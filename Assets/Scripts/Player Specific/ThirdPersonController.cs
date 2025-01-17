#if CMPSETUP_COMPLETE
using System;
using System.Collections;
using AvocadoShark;
using Cinemachine;
using Fusion;
using StylizedWater2;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;

using Unity.VisualScripting.Antlr3.Runtime.Tree;
using Unity.Mathematics;



#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.VFX;
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
        public GameObject eggVisual;
        [SerializeField] public float boostSwimSpeed = 100f;
        [SerializeField] public float defaultSwimSpeed = 50f;
        [HideInInspector] public PlayerManager playerManager;
        [HideInInspector] public float speed;
        private bool attractToEntity;
        private bool pushingAway;
        private Transform currentAttractEntity;
        private const float maxAttractTime = 1;
        private const float maxAttractCooldown = 2;
        private float currentAttractTime;
        private float currentAttractCooldown;
        private const float jumpCooldown = 0.5f;
        private float currentJumpCooldown;

        private Rigidbody rb; 
        private Transform swimArea;
        private bool foundSwimArea;
        [HideInInspector] public CapsuleCollider capsuleCollider;
        [SerializeField] private float waterLevelY;
        private bool outOfWater;
        private ParticleSystem outOfWaterParticles;
        private ParticleSystem insideWaterParticles;

        [Header("Boost")]
        [HideInInspector] public float currentBoostCount;
        [SerializeField] private float boostDelayAfterActivation = 3f;
        [SerializeField] public float boostReloadSpeed = 18;
        [SerializeField] private float boostConsumptionSpeed = 30;
        [SerializeField] private VisualEffect boostParticles;
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
        private float scrollValue = 0;
        private float maxScrollValue = 0.8f;
        private float minScrollValue = -2;
        private float zoomSpeed = 5;

        private GetPlayerCameraAndControls getPlayerCameraAndControls;
        private bool hasVCam = true;

        [Header("Animation")]
        public Animator animator;
        private int animIDMotionSpeed;
        private float defaultAnimSpeed = 2.5f, boostAnimSpeed = 5, notMovingAnimSpeed = 1;

        private SetUIActivationState setUIActivationState;

        private bool _isInHUD = false;
        private PlayerSwimArea _playerSwimArea;

        private float checkObstacleDistance = 0.5f;
        [SerializeField] private LayerMask obstacleLayer;

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
            outOfWaterParticles = GameObject.Find("WaterRipples_Stationairy").GetComponent<ParticleSystem>();
            insideWaterParticles = GameObject.Find("WaterSplash_Ring").GetComponent<ParticleSystem>();
            AudioManager.Instance.PlaySoundAtPosition("impactWithWater", playerVisual.transform.position);
            foundSwimArea = true;
            setUIActivationState.DeactiveLoadPanel();
            _playerSwimArea = swimArea.GetComponent<PlayerSwimArea>();

            playerManager.levelUp.levelUpEvent += CameraForwardRotation;
            _cinemachineFramingTransposer = getPlayerCameraAndControls.vCam.GetCinemachineComponent<CinemachineFramingTransposer>();

        }

        private void Update()
        {
            if (playerManager.levelUp.isEgg || playerManager.playerHealth.isDead || !HasStateAuthority)
                return;

            Gravity();
            SpeedBoost();
            SetActiveMultiplayerUI();
            // ToggleInput();
        }

        public void FixedUpdate()
        {
            if (playerManager.playerHealth.isDead || !HasStateAuthority || !foundSwimArea)
                return;

            if (!playerManager.levelUp.isEgg && !outOfWater)
                Move();
            else
                Jump();
        }

        private void LateUpdate()
        {
            if (playerManager.playerHealth.isDead || !HasStateAuthority)
                return;

            CameraRotation();
        }

        private void SetActiveMultiplayerUI()
        {
            switch (input.setActiveStateMultiplayerUI)
            {
                case true when setUIActivationState.pressedActivationUIMultiplayerButton == false:
                    setUIActivationState.SetActiveUIObjects(ToggleInput);
                    break;
                case false:
                    setUIActivationState.pressedActivationUIMultiplayerButton = false;
                    break;
            }
        }

        private void ToggleInput()
        {
            _isInHUD = !_isInHUD;
            
            if(_isInHUD)
                input.DisablePlayerInput();
            else
                input.EnablePlayerInput();
        }

        private void CameraForwardRotation()
        {
            if (playerManager.levelUp.GetLevel() == 1)
                cineMachineTargetPitch = 20;
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


                if (!playerManager.levelUp.isEgg)
                {
                    if (settingsSO.yInputIsInverted)
                    {
                        cineMachineTargetPitch -= -input.look.y * deltaTimeMultiplier * sensitivity;
                    }
                    else
                    {
                        cineMachineTargetPitch += -input.look.y * deltaTimeMultiplier * sensitivity;
                    }
                }
                else
                {
                    cineMachineTargetPitch = -40;
                }
            }

            // clamp our rotations so our values are limited 360 degrees
            cineMachineTargetYaw = ClampAngle(cineMachineTargetYaw, float.MinValue, float.MaxValue);
            cineMachineTargetPitch = ClampAngle(cineMachineTargetPitch, bottomCameraClamp, topCameraClamp);

            // CineMachine will follow this target
            if (hasVCam)
            {
                var localRotation = transform.localRotation;
                localRotation = Quaternion.Lerp(localRotation, Quaternion.Euler(cineMachineTargetPitch, cineMachineTargetYaw, 0.0f), playerRotationSmoothTime * Time.deltaTime);
                transform.localRotation = localRotation;

                Transform cameraTransform = getPlayerCameraAndControls.vCamRoot.transform;
                float yRotationDifference = Mathf.DeltaAngle(localRotation.eulerAngles.y, cameraTransform.eulerAngles.y);

                float currentCameraRotationSmoothTime = cameraRotationSmoothTime;
                if (yRotationDifference > 90 || yRotationDifference < -90)
                {
                    currentCameraRotationSmoothTime *= 2;
                }

                cameraTransform.localRotation = Quaternion.Lerp(cameraTransform.localRotation, localRotation, currentCameraRotationSmoothTime * Time.deltaTime);

            }
        }
        quaternion randomRotation;
        private CinemachineFramingTransposer _cinemachineFramingTransposer;

        private void Jump()
        {
            if (hasVCam && input.jump && currentJumpCooldown <= 0)
            {
                animator.SetTrigger("jump");
                int impulseForce = 8;
                rb.AddForce(GetRandomDirection() * impulseForce, ForceMode.Impulse);
                RandomizeRotation();
                input.jump = false;
                currentJumpCooldown = jumpCooldown;
                if(playerManager.levelUp.isEgg)
                    playerManager.levelUp.AddExperience(50);

            }
            currentJumpCooldown -= Time.deltaTime;
            rb.useGravity = true;
            rb.drag = 0.05f;
            eggVisual.transform.localRotation = Quaternion.Lerp(eggVisual.transform.localRotation, randomRotation, playerRotationSmoothTime / 3 * Time.deltaTime);

            Vector3 GetRandomDirection()
            {
                float randomX = UnityEngine.Random.Range(-1f, 1f);
                float randomY = UnityEngine.Random.Range(0.5f, 2f);
                float randomZ = UnityEngine.Random.Range(-1f, 1f);

                Vector3 randomDirection = new Vector3(randomX, randomY, randomZ);

                return randomDirection.normalized;
            }
            void RandomizeRotation()
            {
                float randomX = UnityEngine.Random.Range(200, 360);
                float randomY = UnityEngine.Random.Range(200, 360);
                float randomZ = UnityEngine.Random.Range(200, 360);

                randomRotation = Quaternion.Euler(randomX, randomY, randomZ);
            }
        }


        private void Move()
        {
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
            float AnimationSpeed = notMovingAnimSpeed;

            // set target speed based on move speed, sprint speed and if sprint is pressed
            // TODO: Switch Vector3.Distance to sqrMagnitude comparison
            if (Vector3.Distance(transform.position, swimArea.position) >= _playerSwimArea.swimLength && !IsSwimmingTowardIsland())
            {
                if (isBoosting && boostSwimSpeed > 0)
                {
                    speed = boostSwimSpeed - (Vector3.Distance(transform.position, swimArea.position) - _playerSwimArea.swimLength) * 2.5f;
                }
                else if (defaultSwimSpeed > 0)
                {
                    speed = defaultSwimSpeed - (Vector3.Distance(transform.position, swimArea.position) - _playerSwimArea.swimLength);
                }
            }
            else
            {
                speed = isBoosting ? boostSwimSpeed : defaultSwimSpeed;
            }

            var moveDistance = speed * Time.deltaTime;

            if (playerManager.slowDownManager.CurrentlySlowedDown)
            {
                moveDistance /= 2;
            }


            void MovePlayer(float playerRenderRotationX, float playerRenderRotationY)
            {
                Vector3 direction;
                
                if(attractToEntity)
                    direction = (transform.position - currentAttractEntity.position).normalized * (-1 * (boostSwimSpeed * Time.deltaTime));
                else
                    direction = transform.forward * (-1 * (inputDirectionNormalized.z * moveDistance));
            
                if (hasVCam)
                {
                    if (!playerManager.healthManager.IsGrasped && !Physics.Raycast(playerVisual.transform.position, direction, checkObstacleDistance, obstacleLayer))
                        rb.AddForce(direction, ForceMode.Impulse);

                    AnimationSpeed = isBoosting ? boostAnimSpeed : defaultAnimSpeed;
                    getPlayerCameraAndControls.vCam.m_Lens.FieldOfView = Mathf.Lerp(getPlayerCameraAndControls.vCam.m_Lens.FieldOfView, isBoosting ? boostSpeedFOV : defaultSpeedFOV, cameraFOVSmoothTime * Time.deltaTime);
                    _cinemachineFramingTransposer.m_CameraDistance = CameraDistance();
                }

                playerVisual.transform.localRotation = Quaternion.Lerp(playerVisual.transform.localRotation, Quaternion.Euler(playerRenderRotationX, playerRenderRotationY, 0), playerRotationSmoothTime * Time.deltaTime);
            }

            if(!currentAttractEntity || currentAttractTime <= 0)
                attractToEntity = false;

            if(currentAttractCooldown > 0)
                currentAttractCooldown -= Time.deltaTime;

            if(attractToEntity)
            {
                currentAttractTime -= Time.deltaTime;
                if(Vector3.Distance(transform.position,currentAttractEntity.position) > 1)
                    MovePlayer(25, 0);
            }
            else if (inputDirectionNormalized.z >= 0.1 )
            {
                MovePlayer(25, 0);
            }
            else if (inputDirectionNormalized.z <= -0.1)
            {
                MovePlayer(0, -180);
            }
            else
            {
                if (hasVCam)
                    getPlayerCameraAndControls.vCam.m_Lens.FieldOfView = Mathf.Lerp(getPlayerCameraAndControls.vCam.m_Lens.FieldOfView, notMovingFOV, cameraFOVSmoothTime * Time.deltaTime);
            }

            animator.SetFloat(animIDMotionSpeed, AnimationSpeed);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void GraspedRpc(NetworkTransform predator )
        {
            if(playerManager.healthManager.IsGrasped)
                StartAttractToEntity(predator.transform);
        } 

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void PushAwayAnimationRpc()
        {
            boostParticles.Play();
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if(!stateInfo.IsName("Fish_pushAway"))
                animator.SetTrigger("pushAway");
        } 
        public void PushAway(Vector3 Direction)
        {
            rb.AddForce(Direction * 50, ForceMode.Impulse);
            PushAwayAnimationRpc();
        }
        public void StartAttractToEntity(Transform entity)
        {
            if(currentAttractCooldown > 0)
                return;
            currentAttractCooldown = maxAttractCooldown;
            currentAttractEntity = entity;
            currentAttractTime = maxAttractTime;
            attractToEntity = true;
        }
        private float CameraDistance()
        {
            scrollValue += Input.GetAxis("Mouse ScrollWheel");
            scrollValue = Mathf.Clamp(scrollValue, minScrollValue, maxScrollValue);
            float scrollCameraDistance = cameraDistance - scrollValue * zoomSpeed;
            float currentCameraDistance = scrollCameraDistance;
            RaycastHit hit;

            if (Physics.Raycast(playerVisual.transform.position, playerVisual.transform.forward, out hit, scrollCameraDistance, obstacleLayer))
                currentCameraDistance = Vector3.Distance(transform.position, hit.point);

            return currentCameraDistance;
        }

        private void SpeedBoost()
        {
            switch (boostState)
            {
                case BoostState.BoostStarted:
                    if (currentBoostCount <= 0)
                    {
                        boostState = BoostState.BoostReload;
                    }
                    else
                    {
                        if (!isBoosting)
                        {
                            AudioManager.Instance.PlaySoundWithRandomPitchAtPosition("boost", transform.position);
                            boostParticles.Play();
                        }

                        isBoosting = true;
                        if (!permanentStamina)
                            currentBoostCount -= Time.deltaTime * boostConsumptionSpeed;
                    }
                    break;
                case BoostState.BoostReload:
                    if (boostParticles.aliveParticleCount > 8)
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
            if (foundSwimArea && transform.position.y > waterLevelY)
            {
                rb.useGravity = true;
                rb.drag = .5f;

                // The 60 variable is hard coded as a slow speed where the fish should not jump out of water
                if (rb.velocity.sqrMagnitude > 60)
                {
                    outOfWater = true;
                    AudioManager.Instance.PauseSound("underwaterAmbience");
                    var countLoaded = SceneManager.sceneCount;
                    var loadedScenes = new Scene[countLoaded];

                    for (var i = 0; i < countLoaded; i++)
                    {
                        loadedScenes[i] = SceneManager.GetSceneAt(i);
                        if (loadedScenes[i].name == "Lake")
                        {
                            AudioManager.Instance.UnPauseSound("OutOfWaterLake");
                        }
                        else if (loadedScenes[i].name == "Ocean")
                        {
                            AudioManager.Instance.UnPauseSound("OutOfWaterOcean");
                        }
                    }
                    outOfWaterParticles.transform.position = transform.position;
                    outOfWaterParticles.Play();
                    var velocity = rb.velocity;
                    playerVisual.transform.forward = -velocity;
                    cineMachineTargetPitch = -velocity.x;
                    getPlayerCameraAndControls.vCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = CameraDistance() / 2;
                }
            }
            else
            {
                if (outOfWater)
                {
                    ResetPositionAfterOutOfWater();
                    insideWaterParticles.transform.position = transform.position;
                    insideWaterParticles.Play();
                    AudioManager.Instance.UnPauseSound("underwaterAmbience");

                    var countLoaded = SceneManager.sceneCount;
                    var loadedScenes = new Scene[countLoaded];

                    for (var i = 0; i < countLoaded; i++)
                    {
                        loadedScenes[i] = SceneManager.GetSceneAt(i);
                        if (loadedScenes[i].name == "Lake")
                        {
                            AudioManager.Instance.PauseSound("OutOfWaterLake");
                        }
                        else if (loadedScenes[i].name == "Ocean")
                        {
                            AudioManager.Instance.PauseSound("OutOfWaterOcean");
                        }
                    }
                    AudioManager.Instance.PlaySoundAtPosition("impactWithWater", playerVisual.transform.position);
                    outOfWater = false;
                }
                getPlayerCameraAndControls.vCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = CameraDistance();
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
            if (input.move.y is > 0 or < 0)
                yield break;

            input.move.y = 1;
            yield return new WaitForSeconds(.3f);
            input.move.y = 0;
        }
    }
}
#endif