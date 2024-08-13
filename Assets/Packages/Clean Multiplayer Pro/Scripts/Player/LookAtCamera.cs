using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvocadoShark
{
    public class LookAtCamera : MonoBehaviour
    {
        private GameObject mainCamera;

        private void Update() 
        {
            var virtualCamera = GameObject.Find("Virtual Camera");
            transform.LookAt(virtualCamera.transform);
        }
    }
}
