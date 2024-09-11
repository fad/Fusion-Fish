#if CMPSETUP_COMPLETE
using System.Collections;
using Cinemachine;
using UnityEngine;
using Fusion;
using StarterAssets;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace AvocadoShark
{
    public class GetPlayerCameraAndControls : NetworkBehaviour
    {
        [SerializeField] StarterAssetsInputs AssetInputs;
        [SerializeField] PlayerInput PlayerInput;
        [SerializeField] Transform PlayerModel;
        [SerializeField] Transform InterpolationPoint;
        private Rigidbody _rigidbody;
        public bool UseMobileControls;
        [HideInInspector] public CinemachineVirtualCamera vCam;
        [HideInInspector] public Transform vCamRoot;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void Spawned()
        {
            var thirdPersonController = GetComponent<ThirdPersonController>();
            if (HasStateAuthority)
            {
                _rigidbody.MovePosition(new Vector3(Random.Range(-7.6f, 14.2f), 0,
                    Random.Range(-31.48f, -41.22f)));

                vCam = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
                vCamRoot = GameObject.Find("Cams").GetComponent<Transform>();
                vCam.LookAt = transform;
                vCam.Follow = transform;
                
                if (UseMobileControls)
                {
                    var mobileControls = GameObject.Find("Mobile Controls");
                    mobileControls.GetComponent<UICanvasControllerInput>().starterAssetsInputs = AssetInputs;
                    mobileControls.GetComponent<MobileDisableAutoSwitchControls>().playerInput = PlayerInput;
                }

                StartCoroutine(EnableTpc());
                IEnumerator EnableTpc()
                {
                    transform.position = new Vector3(Random.Range(-7.6f, 14.2f), 0,
                        Random.Range(-31.48f, -41.22f));
                    yield return null;
                    thirdPersonController.enabled = true;
                }
            }
            else
            {
                PlayerModel.SetParent(InterpolationPoint);
            }
        }
    }
}
#endif
