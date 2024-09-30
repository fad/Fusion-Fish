#if CMPSETUP_COMPLETE
using System.Collections;
using Cinemachine;
using UnityEngine;
using Fusion;
using StarterAssets;
using UnityEngine.InputSystem;

namespace AvocadoShark
{
    public class GetPlayerCameraAndControls : NetworkBehaviour
    {
        [SerializeField] StarterAssetsInputs assetInputs;
        [SerializeField] PlayerInput playerInput;
        [SerializeField] Transform playerModel;
        [SerializeField] Transform interpolationPoint;
        public bool useMobileControls;
        [HideInInspector] public CinemachineVirtualCamera vCam;
        [HideInInspector] public Transform vCamRoot;

        public override void Spawned()
        {
            var thirdPersonController = GetComponent<ThirdPersonController>();
            if (HasStateAuthority)
            {
                vCam = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
                vCamRoot = GameObject.Find("Cams").GetComponent<Transform>();
                var thisTransform = transform;
                vCam.LookAt = thisTransform;
                vCam.Follow = thisTransform;
                
                if (useMobileControls)
                {
                    var mobileControls = GameObject.Find("Mobile Controls");
                    mobileControls.GetComponent<UICanvasControllerInput>().starterAssetsInputs = assetInputs;
                    mobileControls.GetComponent<MobileDisableAutoSwitchControls>().playerInput = playerInput;
                }

                StartCoroutine(EnableTpc());
                IEnumerator EnableTpc()
                {
                    yield return null;
                    thirdPersonController.enabled = true;
                }
            }
            else
            {
                playerModel.SetParent(interpolationPoint);
            }
        }
    }
}
#endif
