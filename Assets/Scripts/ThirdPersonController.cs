
#if CMPSETUP_COMPLETE
using System;
using System.Collections;
using Cinemachine;
using Fusion;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : NetworkBehaviour
    {
        [Header("Boost")]
        [HideInInspector] public float currentBoostCount;
        [SerializeField] private float boostDelayAfterActivation = 3f;
        public float maxBoostCount = 100f;
        private bool isBoosting;
        private bool canReloadBoost = true;
        
        [Space(10)]

        [Header("CineMachine")]
        [Tooltip("How fast the fov changes when the character speed changes")]
        [SerializeField] private float cameraFOVSmoothTime = 3f;
        [Tooltip("The follow target set in the CineMachine Virtual Camera that the camera will follow")]
        [HideInInspector] public GameObject cineMachineCameraTarget;
        [SerializeField] private CinemachineVirtualCamera vCam;
        [SerializeField] private Transform vCamRoot;
        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] private float topClamp = 70.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        [SerializeField] private float bottomClamp = -30.0f;
        [Tooltip("Additional degrees to override the camera. Useful for fine tuning camera position when locked")]
        [SerializeField] private float cameraAngleOverride;
        [Tooltip("For locking the camera position on all axis")]
        [SerializeField] private bool lockCameraPosition;
        private float cineMachineTargetYaw;
        private float cineMachineTargetPitch;
        
        [Space(10)]

        [Header("Movement")]
        [Tooltip("How fast the character turns to face movement direction")]
        [SerializeField] private float rotationSmoothTime = 3f;
        [Tooltip("How fast you can rotate the player depending on the mouse movement, the camera moves with the player")]
        [SerializeField] private float sensitivity = .85f;
        [SerializeField] private GameObject playerVisual;
        [SerializeField] private float boostSwimSpeed = 100f;
        [SerializeField] private float defaultSwimSpeed = 50f;
        [HideInInspector] public PlayerManager playerManager;
        private float speed;
        private float rotationVelocity;
        private bool outOfWater;
        private Rigidbody rb;

        [Header("Animation")]
        public Animator animator;
        private int animIDMotionSpeed;
        
        
        
#if ENABLE_INPUT_SYSTEM
        private PlayerInput playerInput;
#endif
        [HideInInspector] public StarterAssetsInputs input;
        
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }
        
        [HideInInspector] public BoostState boostState;
        [HideInInspector] public enum BoostState
        {
            BoostFull,
            BoostStarted,
            BoostReload,
        }

        private void Start()
        {
            cineMachineCameraTarget = gameObject;
            playerManager = GetComponent<PlayerManager>();
            cineMachineTargetYaw = cineMachineCameraTarget.transform.rotation.eulerAngles.y;
            input = GetComponent<StarterAssetsInputs>();
            boostState = BoostState.BoostReload;
            rb = GetComponent<Rigidbody>();
            vCamRoot = GameObject.Find("Cams").GetComponent<Transform>();
            vCam = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
            
#if ENABLE_INPUT_SYSTEM
            playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            animIDMotionSpeed = Animator.StringToHash("movingSpeed");
        }

        private void Update()
        {
            if(playerManager.health.isDead)
                return;
            
            Gravity();
            SpeedBoost();
            
            if (HasStateAuthority == false)
            {
                return;
            }
        }

        private void FixedUpdate()
        {
            if (playerManager.health.isDead)
                return;
            
            Move();
        }

        private void LateUpdate()
        {
            if(playerManager.health.isDead)
                return;
            
            CameraRotation();
        }

        private void CameraRotation()
        {
            if (!lockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                cineMachineTargetYaw += input.look.x * deltaTimeMultiplier * sensitivity;
                cineMachineTargetPitch += -input.look.y * deltaTimeMultiplier * sensitivity;
            }

            // clamp our rotations so our values are limited 360 degrees
            cineMachineTargetYaw = ClampAngle(cineMachineTargetYaw, float.MinValue, float.MaxValue);
            cineMachineTargetPitch = ClampAngle(cineMachineTargetPitch, bottomClamp, topClamp);

            // CineMachine will follow this target
            var localRotation = transform.localRotation;
            localRotation = Quaternion.Lerp(localRotation, Quaternion.Euler(cineMachineTargetPitch + cameraAngleOverride, cineMachineTargetYaw, 0.0f), rotationSmoothTime * Time.deltaTime);
            transform.localRotation = localRotation;
            vCamRoot.transform.localRotation = Quaternion.Lerp(vCamRoot.transform.localRotation, localRotation, rotationSmoothTime * Time.deltaTime);
        }

        private void Move()
        {
            if (input.sprint)
            {
                boostState = BoostState.BoostStarted;
            }
            else if (!input.sprint && isBoosting)
            {
                boostState = BoostState.BoostReload;
            }

            Vector3 inputDirectionNormalized = new Vector3(input.move.x, 0.0f, input.move.y).normalized;
            // set target speed based on move speed, sprint speed and if sprint is pressed
            speed = isBoosting ? boostSwimSpeed : defaultSwimSpeed;
            
            var moveDistance = speed * Time.deltaTime;

            void MovePlayer(float playerRenderRotation)
            {
                rb.AddForce(vCam.transform.forward * (inputDirectionNormalized.z * moveDistance), ForceMode.Impulse);

                vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, isBoosting ? 30f : 20f, cameraFOVSmoothTime * Time.deltaTime);

                if (!playerManager.attack.isAttacking)
                {
                    playerVisual.transform.localRotation = Quaternion.Lerp(playerVisual.transform.localRotation, Quaternion.Euler(playerRenderRotation, 0, playerRenderRotation), rotationSmoothTime * Time.deltaTime);
                }
            }
            
            if (inputDirectionNormalized.z >= 0.1)
            {
                MovePlayer(0);
            }
            else if (inputDirectionNormalized.z <= -0.1)
            {
                MovePlayer(-180);
            }
            else
            {
                vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, 17.5f, cameraFOVSmoothTime * Time.deltaTime);
            }
        
            animator.SetFloat(animIDMotionSpeed, rb.velocity.sqrMagnitude);
        }

        private void SpeedBoost()
        {
            switch (boostState)
            {
                case BoostState.BoostFull :
                    break;
                case BoostState.BoostStarted :
                    isBoosting = true;
                    currentBoostCount -= Time.deltaTime * 30f;
                    if (currentBoostCount <= 0)
                    {
                        boostState = BoostState.BoostReload;
                    }
                    break;
                case BoostState.BoostReload :
                    if (isBoosting)
                    {
                        StartCoroutine(DelayedBoostReloadCoroutine());
                    }
                    isBoosting = false;
                    if (canReloadBoost)
                    {
                        currentBoostCount += Time.deltaTime * 18f;
                    }
                    
                    if (currentBoostCount >= maxBoostCount)
                    {
                        boostState = BoostState.BoostFull;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private IEnumerator DelayedBoostReloadCoroutine()
        {
            yield return new WaitForSeconds(boostDelayAfterActivation);

            canReloadBoost = true;
        }

        private void Gravity()
        {
            rb.useGravity = outOfWater;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}
#endif