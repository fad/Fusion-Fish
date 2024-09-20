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
        public void ResetPlayerPosition()
        {
            transform.SetPositionAndRotation(SpawnManager.Instance.chosenLocation, SpawnManager.Instance.chosenRotation);
        }
    }
}
#endif