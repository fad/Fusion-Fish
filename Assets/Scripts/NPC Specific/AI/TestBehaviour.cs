using System;
using System.Linq;
using UnityEngine;

public class TestBehaviour : MonoBehaviour, ITreeRunner
{
    [SerializeField] private FishData fishData;
    
    private bool _isHunting;
    private bool _isInDanger;
    
    private Transform _target;

    public FishData FishType => fishData;

    private void Update()
    {
        if (_isHunting)
        {
            Debug.Log($"{gameObject.name} currently hunting {_target.name}".InColor(Color.red));
        }
        
        if (_isInDanger)
        {
            Debug.Log($"{gameObject.name} currently fleeing from {_target.name}".InColor(Color.blue));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(120f/255f, 33f/255f, 114f/255f, 1f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }

    public void AdjustAreaCheck((bool isInside, Vector3 direction) areaCheck)
    {
        throw new System.NotImplementedException();
    }

    public void AdjustHuntOrFleeTarget((Transform targetTransform, ITreeRunner targetBehaviour) targetData)
    {
        if (_isHunting || _isInDanger) return;
        if (targetData.targetBehaviour.FishType == FishType) return; // if the other fish type is the same as this one
        
        
        
        if (FishType.PredatorList.Contains(targetData.targetBehaviour.FishType) && !_isHunting)
        {
            _isInDanger = true;
            _target = targetData.targetTransform;
            return;
        }
        
        if (FishType.PreyList.Contains(targetData.targetBehaviour.FishType) && !_isInDanger)
        {
            _isHunting = true;
            _target = targetData.targetTransform;
        
        }
    }
}
