#if CMPSETUP_COMPLETE
using System;
using System.Collections;
using AvocadoShark;
using Cinemachine;
using Fusion;
using StylizedWater2;
using Unity.VisualScripting;
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
        [SerializeField] public float boostSwimSpeed = 100f;
        [SerializeField] public float defaultSwimSpeed = 50f;
        [SerializeField] private float  maxSwimAreaLength;
        [HideInInspector] public PlayerManager playerManager;
        [HideInInspector] public float speed;
        private Rigidbody rb;
        private Transform swimArea;
        private bool foundSwimArea;
        [HideInInspector] public CapsuleCollider capsuleCollider;
        [SerializeField] private float waterLevelY;
        private bool outOfWater;
        
        [Header("Boost")]
        [HideInInspector] public float currentBoostCount;
        [SerializeField] private float boostDelayAfterActivation = 3f;
        [SerializeField] private float boostReloadSpeed = 18;
        [SerializeField] private float boostConsumptionSpeed = 30;
        [SerializeField] private ParticleSystem boostParticles;
        [HideInInspector] public bool permanentStamina;
        public float maxBoostCount = 100f;
        private bool isBoosting;
        private bool canReloadBoost = true;
        private bool canReload;

        [Header("CineMachine")]
        [Tooltip("How fast the fov changes when the character speed changes")]
        [SerializeField] private float cameraFOVSmoothTime = 3f;
        [Tooltip("How fast the camera rotates with the player")]
        [SerializeField] private float cameraRotationSmoothTime = 3f;
        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] private float topCameraClamp = 70.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        [SerializeField] private float bottomCameraClamp = -30.0f;
        [Tooltip("For locking the camera position on all axis")]
        [SerializeField] private bool lockCameraPosition;
        public float notMovingFOV = 17.5f;
        public float defaultSpeedFOV = 20f;
        public float boostSpeedFOV = 35;
        public float cameraDistance = 5;
        private float cineMachineTargetYaw;
        private float cineMachineTargetPitch;
        private GetPlayerCameraAndControls getPlayerCameraAndControls;
        private bool hasVCam = true;

        [Header("Animation")]
        public Animator animator;
        private int animIDMotionSpeed;

        private SetUIActivationState setUIActivationState;
        
        
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
        public enum BoostState
        {
            BoostStarted,
            BoostReload,
        }

        private IEnumerator Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            capsuleCollider = GetComponent<CapsuleCollider>();

            playerManager = GetComponent<PlayerManager>();
            input = GetComponent<StarterAssetsInputs>();
            rb = GetComponent<Rigidbody>();
            setUIActivationState = GameObject.Find("SetUIActivationState").GetComponent<SetUIActivationState>();
            getPlayerCameraAndControls = GetComponent<GetPlayerCameraAndControls>();
            if (getPlayerCameraAndControls.vCamRoot == null)
                hasVCam = false;
            
            cineMachineTargetYaw = gameObject.transform.rotation.eulerAngles.y;
            
            boostState = BoostState.BoostReload;
            currentBoostCount = maxBoostCount;

#if ENABLE_INPUT_SYSTEM
            playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            animIDMotionSpeed = Animator.StringToHash("movingSpeed");
            
            yield return new WaitUntil(() => GameObject.Find("SwimArea") != null);
            swimArea = GameObject.Find("SwimArea").GetComponent<Transform>();
            waterLevelY = FindObjectOfType<WaterGrid>().transform.position.y;
            AudioManager.Instance.PlaySoundAtPosition("impactWithWater", playerVisual.transform.position);
            foundSwimArea = true;
        }

        private void Update()
        {
            if(playerManager.playerHealth.isDead || !HasStateAuthority)
                return;
            
            Gravity();
            SpeedBoost();
            SetActiveMultiplayerUI();
        }

        public void FixedUpdate()
        {
            if (playerManager.playerHealth.isDead || !HasStateAuthority || !foundSwimArea || outOfWater)
                return;
            
            Move();        
        }

        private void LateUpdate()
        {
            if(playerManager.playerHealth.isDead || !HasStateAuthority)
                return;
            
            CameraRotation();
        }

        private void SetActiveMultiplayerUI()
        {
            switch (input.setActiveStateMultiplayerUI)
            {
                case true when setUIActivationState.pressedActivationUIMultiplayerButton == false:
                    setUIActivationState.SetActiveUIObjects();
                    break;
                case false:
                    setUIActivationState.pressedActivationUIMultiplayerButton = false;
                    break;
            }
        }

        private void CameraRotation()
        {
            if (!lockCameraPosition && !outOfWater)
            {
                //Don't multiply mouse input by Time.deltaTime;
                var deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                if (settingsSO.xInputIsInverted)
                {
                    cineMachineTargetYaw -= input.look.x * deltaTimeMultiplier * sensitivity;
                }
                else
                {
                    cineMachineTargetYaw += Mathf.Clamp(input.look.x, -15, 15) * deltaTimeMultiplier * sensitivity;
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
            cineMachineTargetPitch = ClampAngle(cineMachineTargetPitch, bottomCameraClamp, topCameraClamp);

            // CineMachine will follow this target
            if (hasVCam)
            {
                var localRotation = getPlayerCameraAndControls.vCamRoot.transform.localRotation;
                localRotation = Quaternion.Lerp(localRotation, Quaternion.Euler(cineMachineTargetPitch, cineMachineTargetYaw, 0.0f), cameraRotationSmoothTime * Time.deltaTime);
                getPlayerCameraAndControls.vCamRoot.transform.localRotation = localRotation;

                transform.localRotation = Quaternion.Lerp(transform.localRotation, localRotation, playerRotationSmoothTime * Time.deltaTime);
            }
        }

        private void Move()
        {
            //Debug.Log(rb.velocity.sqrMagnitude < 50);

            //Debug.Log(rb.velocity.sqrMagnitude);
            if (input.sprint && input.move.y is > 0 or < 0)
            { 
                canReload = false;
                
                boostState = BoostState.BoostStarted;
            }
            else
            {
                boostState = BoostState.BoostReload;
            }

            var inputDirectionNormalized = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

            // set target speed based on move speed, sprint speed and if sprint is pressed
            if (Vector3.Distance(transform.position, swimArea.position) >= maxSwimAreaLength && !IsSwimmingTowardIsland())
            {
                if (isBoosting && boostSwimSpeed > 0)
                {
                    speed = boostSwimSpeed - (Vector3.Distance(transform.position, swimArea.position) - maxSwimAreaLength) *  2.5f;
                }
                else if (defaultSwimSpeed > 0)
                {
                    speed = defaultSwimSpeed - (Vector3.Distance(transform.position, swimArea.position) - maxSwimAreaLength);
                }
            }
            else
            {
                speed = isBoosting ? boostSwimSpeed : defaultSwimSpeed;
            }
            
            var moveDistance = speed * Time.deltaTime;

            if (playerManager.healthManager.slowDown)
            {
                moveDistance /= 2;
            }

            void MovePlayer(float playerRenderRotationX, float playerRenderRotationY)
            {
                if (hasVCam)
                {
                    rb.AddForce(getPlayerCameraAndControls.vCam.transform.forward * (inputDirectionNormalized.z * moveDistance), ForceMode.Impulse);

                    getPlayerCameraAndControls.vCam.m_Lens.FieldOfView = Mathf.Lerp(getPlayerCameraAndControls.vCam.m_Lens.FieldOfView, isBoosting ? boostSpeedFOV : defaultSpeedFOV, cameraFOVSmoothTime * Time.deltaTime);
                    getPlayerCameraAndControls.vCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = cameraDistance;
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
                    getPlayerCameraAndControls.vCam.m_Lens.FieldOfView = Mathf.Lerp(getPlayerCameraAndControls.vCam.m_Lens.FieldOfView, notMovingFOV, cameraFOVSmoothTime * Time.deltaTime);
            }
        
            animator.SetFloat(animIDMotionSpeed, rb.velocity.sqrMagnitude);
        }

        private void SpeedBoost()
        {
            switch (boostState)
            {
                case BoostState.BoostStarted :
                    if (currentBoostCount <= 0)
                    {
                        boostState = BoostState.BoostReload;
                    }
                    else
                    { 
                        if(!isBoosting)
                            AudioManager.Instance.PlaySoundWithRandomPitchAtPosition("boost", transform.position);
                        boostParticles.Play();
                        isBoosting = true;
                        if(!permanentStamina)
                            currentBoostCount -= Time.deltaTime * boostConsumptionSpeed;
                    }
                    break;
                case BoostState.BoostReload :
                    boostParticles.Stop();

                    if (!canReload)
                    {
                        DelayedBoostReload();
                    }
                    
                    isBoosting = false;

                    if (currentBoostCount < maxBoostCount && canReloadBoost)
                    {
                        currentBoostCount += Time.deltaTime * boostReloadSpeed;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DelayedBoostReload()
        {
            StartCoroutine(DelayedBoostReloadCoroutine());
        }
        
        private IEnumerator DelayedBoostReloadCoroutine()
        {
            canReload = true;

            canReloadBoost = false;
            
            yield return new WaitForSeconds(boostDelayAfterActivation);
            
            canReloadBoost = true;
        }

        private bool IsSwimmingTowardIsland()
        {
            //where the island is depending on player position
            var direction = transform.position - swimArea.transform.position;
            //gives angle as float from look direction to the direction to the middle of the island
            if (playerVisual)
            {
                var targetAngle = Vector3.Angle(playerVisual.transform.forward, direction);
            
                return targetAngle < 90;   
            }

            return false;
        }

        private void Gravity()
        {
            if (transform.position.y > waterLevelY)
            {
                rb.useGravity = true;
                rb.drag = .5f;

                // The 60 variable is hard coded as a slow speed where the fish should not jump out of water
                if (rb.velocity.sqrMagnitude > 60)
                {
                    outOfWater = true;
                    var velocity = rb.velocity;
                    playerVisual.transform.forward = -velocity;
                    cineMachineTargetPitch = -velocity.x;
                    getPlayerCameraAndControls.vCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = cameraDistance / 2;
                }
            }
            else
            {
                if (outOfWater)
                {
                    ResetPositionAfterOutOfWater();
                    outOfWater = false;
                }
                getPlayerCameraAndControls.vCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = cameraDistance;
                rb.drag = 6;
                rb.useGravity = false;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void ResetPositionAfterOutOfWater()
        {
            StartCoroutine(ResetPositionAfterOutOfWaterCoroutine());
        }

        private IEnumerator ResetPositionAfterOutOfWaterCoroutine()
        {
            if(input.move.y is > 0 or < 0)
                yield break;
            
            input.move.y = 1;
            yield return new WaitForSeconds(.3f);
            input.move.y = 0;
        }
    }
}
#endif