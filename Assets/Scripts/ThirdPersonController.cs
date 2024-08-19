
#if CMPSETUP_COMPLETE
using System;
using System.Collections;
using AvocadoShark;
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
        [Header("PlayerSettings")] 
        [SerializeField] private SettingsSO settingsSO;
        
        [Header("Movement")]
        [Tooltip("How fast the character turns to face movement direction")]
        [SerializeField] private float playerRotationSmoothTime = 3f;
        [Tooltip("How fast you can rotate the player depending on the mouse movement, the camera moves with the player")]
        public float sensitivity = .85f;
        public GameObject playerVisual;
        public GameObject playerMesh;
        [SerializeField] private float boostSwimSpeed = 100f;
        [SerializeField] private float defaultSwimSpeed = 50f;
        [SerializeField] private float  maxSwimAreaLength;
        [HideInInspector] public PlayerManager playerManager;
        private float speed;
        private float rotationVelocity;
        private bool outOfWater;
        private Rigidbody rb;
        private Transform swimArea;
        
        [Header("Boost")]
        [HideInInspector] public float currentBoostCount;
        [SerializeField] private float boostDelayAfterActivation = 3f;
        public float maxBoostCount = 100f;
        private bool isBoosting;
        private bool canReloadBoost = true;
        
        [Header("CineMachine")]
        [Tooltip("How fast the fov changes when the character speed changes")]
        [SerializeField] private float cameraFOVSmoothTime = 3f;
        [Tooltip("How fast the camera rotates with the player")]
        [SerializeField] private float cameraRotationSmoothTime = 3f;
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
        private GetPlayerCameraAndControls getPlayerCameraAndControls;
        private bool hasVCam = true;
        
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
            BoostStarted,
            BoostReload,
        }

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            swimArea = GameObject.Find("SwimArea").GetComponent<Transform>();
            getPlayerCameraAndControls = GetComponent<GetPlayerCameraAndControls>();
            if (getPlayerCameraAndControls.vCamRoot == null)
                hasVCam = false;
            AudioManager.Instance.PlaySoundAtPosition("impactWithWater", transform.position);
            AudioManager.Instance.Play("underwaterAmbience");
            playerManager = GetComponent<PlayerManager>();
            cineMachineTargetYaw = gameObject.transform.rotation.eulerAngles.y;
            input = GetComponent<StarterAssetsInputs>();
            boostState = BoostState.BoostReload;
            rb = GetComponent<Rigidbody>();

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

                if (settingsSO.xInputIsInverted)
                {
                    cineMachineTargetYaw -= input.look.x * deltaTimeMultiplier * sensitivity;
                }
                else
                {
                    cineMachineTargetYaw += input.look.x * deltaTimeMultiplier * sensitivity;
                }

                if (settingsSO.yInputIsInverted)
                {
                    cineMachineTargetPitch -= -input.look.y * deltaTimeMultiplier * sensitivity;
                }
                else
                {
                    cineMachineTargetPitch += -input.look.y * deltaTimeMultiplier * sensitivity;
                }
            }

            // clamp our rotations so our values are limited 360 degrees
            cineMachineTargetYaw = ClampAngle(cineMachineTargetYaw, float.MinValue, float.MaxValue);
            cineMachineTargetPitch = ClampAngle(cineMachineTargetPitch, bottomClamp, topClamp);

            // CineMachine will follow this target
            var localRotation = transform.localRotation;
            localRotation = Quaternion.Lerp(localRotation, Quaternion.Euler(cineMachineTargetPitch + cameraAngleOverride, cineMachineTargetYaw, 0.0f), playerRotationSmoothTime * Time.deltaTime);
            transform.localRotation = localRotation;
            if (hasVCam)
            {
                getPlayerCameraAndControls.vCamRoot.transform.localRotation = Quaternion.Lerp(getPlayerCameraAndControls.vCamRoot.transform.localRotation, localRotation, cameraRotationSmoothTime * Time.deltaTime);
            }
        }

        private void Move()
        {
            if (input.sprint)
            {
                if(boostState != BoostState.BoostStarted)
                    AudioManager.Instance.PlaySoundWithRandomPitchAtPosition("boost", transform.position);
                
                boostState = BoostState.BoostStarted;
            }
            else if (!input.sprint && isBoosting)
            {
                boostState = BoostState.BoostReload;
            }

            Vector3 inputDirectionNormalized = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

            // set target speed based on move speed, sprint speed and if sprint is pressed
            if (Vector3.Distance(transform.position, swimArea.position) >= maxSwimAreaLength && !IsSwimmingTowardIsland())
            {
                if (isBoosting)
                {
                    if (boostSwimSpeed > 0)
                    {
                        speed = boostSwimSpeed - (Vector3.Distance(transform.position, swimArea.position) - maxSwimAreaLength * 2);
                    }
                }
                else
                {
                    if (defaultSwimSpeed > 0)
                    {
                        speed = defaultSwimSpeed - (Vector3.Distance(transform.position, swimArea.position) - maxSwimAreaLength * 2);
                    }
                }
            }
            else
            {
                speed = isBoosting ? boostSwimSpeed : defaultSwimSpeed;
            }

            var moveDistance = speed * Time.deltaTime;

            void MovePlayer(float playerRenderRotationX, float playerRenderRotationY)
            {
                if (hasVCam)
                {
                    rb.AddForce(getPlayerCameraAndControls.vCam.transform.forward * (inputDirectionNormalized.z * moveDistance), ForceMode.Impulse);

                    getPlayerCameraAndControls.vCam.m_Lens.FieldOfView = Mathf.Lerp(getPlayerCameraAndControls.vCam.m_Lens.FieldOfView, isBoosting ? 30f : 20f, cameraFOVSmoothTime * Time.deltaTime);
                }

                playerVisual.transform.localRotation = Quaternion.Lerp(playerVisual.transform.localRotation, Quaternion.Euler(playerRenderRotationX, playerRenderRotationY, 0), playerRotationSmoothTime * Time.deltaTime);
            }
            
            if (inputDirectionNormalized.z >= 0.1)
            {
                MovePlayer(25, 0);
            }
            else if (inputDirectionNormalized.z <= -0.1)
            {
                MovePlayer(0, -180);
            }
            else
            {
                if(hasVCam)
                    getPlayerCameraAndControls.vCam.m_Lens.FieldOfView = Mathf.Lerp(getPlayerCameraAndControls.vCam.m_Lens.FieldOfView, 17.5f, cameraFOVSmoothTime * Time.deltaTime);
            }
        
            animator.SetFloat(animIDMotionSpeed, rb.velocity.sqrMagnitude);
        }

        private void SpeedBoost()
        {
            switch (boostState)
            {
                case BoostState.BoostStarted :
                    if (currentBoostCount < 0)
                    {
                        boostState = BoostState.BoostReload;
                    }
                    else
                    {
                        isBoosting = true;
                        currentBoostCount -= Time.deltaTime * 30f;
                    }
                    break;
                case BoostState.BoostReload :
                    if(isBoosting)
                        StartCoroutine(DelayedBoostReloadCoroutine());

                    isBoosting = false;

                    if (currentBoostCount < maxBoostCount)
                    {
                        if (canReloadBoost)
                        {
                            currentBoostCount += Time.deltaTime * 18f;
                        }
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

        private bool IsSwimmingTowardIsland()
        {
            if (hasVCam)
            {
                //where the island is depending on player position
                var direction = swimArea.transform.position - transform.position;
                //gives angle as float from look direction to the direction to the middle of the island
                var targetAngle = Vector3.Angle(getPlayerCameraAndControls.vCam.transform.forward, direction);
            
                return targetAngle < 90;   
            }
            else
            {
                return true;
            }
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