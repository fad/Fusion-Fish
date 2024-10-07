using UnityEngine;

namespace AvocadoShark
{
    public class LookAtCamera : MonoBehaviour
    {
        protected GameObject VirtualCamera;
        
        protected virtual void Start() 
        {
            VirtualCamera = GameObject.Find("Virtual Camera");
        }

        protected virtual void Update() 
        {
            transform.LookAt(VirtualCamera.transform);
        }
    }
}
