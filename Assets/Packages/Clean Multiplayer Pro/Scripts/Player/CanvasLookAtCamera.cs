using AvocadoShark;
using UnityEngine;

public class CanvasLookAtCamera : LookAtCamera
{
    private RectTransform _rectTransform;
    
    protected override void Start() 
    {
        base.Start();
        _rectTransform = GetComponent<RectTransform>();
    }

    protected override void Update() 
    {
        _rectTransform.LookAt(VirtualCamera.transform);
    }
}
