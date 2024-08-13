using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasLookAtCamera : MonoBehaviour
{
    private RectTransform rectTransform;
    
    private void Start() 
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update() 
    {
        var virtualCamera = GameObject.Find("Virtual Camera");
        rectTransform.LookAt(virtualCamera.transform);
    }
}
