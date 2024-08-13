
#if CMPSETUP_COMPLETE
using System;
using System.Collections;
using Cinemachine;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : NetworkBehaviour
    {
        [Header("Player")]
        [HideInInspector] public float currentBoostCount;
        public float maxBoostCount = 100f;
        [SerializeField] private float boostDelayAfterActivation = 3f;
        [SerializeField] private float boostSwimSpeed = 100f;
        [SerializeField] private float defaultSwimSpeed = 50f;
        [SerializeField] private GameObject playerVisual;
        [SerializeField] private Attack attack;

        [Tooltip("How fast the character turns to face movement direction")]
        [SerializeField] private float rotationSmoothTime = 3f;
        

        public AudioClip landingInWaterAudioClip;
        public AudioClip[] finStrokeAudioClips;
        [Range(0, 1)] public float finStrokeAudioVolume = 0.5f;

        [Space(10)]
        [Header("CineMachine")]
        [Tooltip("The follow target set in the CineMachine Virtual Camera that the camera will follow")]
        public GameObject cineMachineCameraTarget;

        [SerializeField] private CinemachineVirtualCamera vCam;
        [SerializeField] private Transform vCamRoot;
        
        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] private float sensitivity = .85f;

        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] private float topClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        [SerializeField] private float bottomClamp = -30.0f;

        [Tooltip("Additional degrees to override the camera. Useful for fine tuning camera position when locked")]
        [SerializeField] private float cameraAngleOverride;

        [Tooltip("For locking the camera position on all axis")]
        [SerializeField] private bool lockCameraPosition;

        // cineMachine
        private float cineMachineTargetYaw;
        private float cineMachineTargetPitch;

        // player
        private float speed;
        private bool isBoosting;
        private bool canReloadBoost = true;
        private float rotationVelocity;
        private bool outOfWater;
        private Rigidbody rb;

        // timeout deltaTime
        private float jumpTimeoutDelta;
        private float fallTimeoutDelta;

        // animation IDs
        private int animIDMotionSpeed;
        [SerializeField] private Animator animator;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput playerInput;
#endif
        [HideInInspector] public StarterAssetsInputs input;
        private GameObject mainCamera;
        
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
        
        private BoostState boostState;
        private enum BoostState
        {
            BoostFull,
            BoostStarted,
            BoostReload,
        }
        
        private void Awake()
        {
            // get a reference to our main camera
            if (mainCamera == null)
            {
                mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }
        
        private void Start()
        {
            cineMachineTargetYaw = cineMachineCameraTarget.transform.rotation.eulerAngles.y;
            input = GetComponent<StarterAssetsInputs>();
            boostState = BoostState.BoostReload;
            rb = GetComponent<Rigidbody>();
            vCamRoot = GameObject.Find("Cams").GetComponent<Transform>();
            vCam = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
            vCam.Follow = transform;
            vCam.LookAt = transform;
#if ENABLE_INPUT_SYSTEM
            playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();
        }

        private void Update()
        {
            Gravity();
            SpeedBoost();
            
            if (HasStateAuthority == false)
            {
                return;
            }
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            animIDMotionSpeed = Animator.StringToHash("movingSpeed");
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
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
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(cineMachineTargetPitch + cameraAngleOverride, cineMachineTargetYaw, 0.0f), rotationSmoothTime * Time.deltaTime);
            vCamRoot.transform.localRotation = transform.localRotation;
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
            
            if (inputDirectionNormalized.z >= 0.1)
            {
                rb.AddForce(vCam.transform.forward * (inputDirectionNormalized.z * moveDistance), ForceMode.Impulse);
                
                if (!attack.isAttacking)
                {
                    playerVisual.transform.localRotation = Quaternion.Lerp(playerVisual.transform.localRotation, Quaternion.Euler(0, 0, 0), rotationSmoothTime * Time.deltaTime);
                }
            }
            else if (input.move.y <= -0.1)
            {
                rb.AddForce(vCam.transform.forward * (inputDirectionNormalized.z * moveDistance), ForceMode.Impulse);

                if (!attack.isAttacking)
                {
                    playerVisual.transform.localRotation = Quaternion.Lerp(playerVisual.transform.localRotation, Quaternion.Euler(-180, 0, -180), rotationSmoothTime * Time.deltaTime);
                }
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

        private void OnFinStroke(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (finStrokeAudioClips.Length > 0)
                {
                    var index = Random.Range(0, finStrokeAudioClips.Length);
                    if (enabled)
                        AudioSource.PlayClipAtPoint(finStrokeAudioClips[index], transform.position, finStrokeAudioVolume);
                }
            }
        }

        private void OnImpactWithWater(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (enabled)
                    AudioSource.PlayClipAtPoint(landingInWaterAudioClip, transform.position, finStrokeAudioVolume);
            }
        }
    }
}
#endif