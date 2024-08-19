#if CMPSETUP_COMPLETE
using Fusion;
using UnityEngine;

namespace AvocadoShark
{
    public class PlayerPosResetter : NetworkBehaviour
    {
        public float minYValue = -10f;

        void LateUpdate()
        {
            if (HasStateAuthority)
            {
                if (transform.position.y < minYValue)
                {
                    ResetPlayerPosition();
                }
            }
        }
        void ResetPlayerPosition()
        {
            transform.SetPositionAndRotation(FusionConnection.Instance.chosenLocation, FusionConnection.Instance.chosenRotation);
        }
    }
}
#endif