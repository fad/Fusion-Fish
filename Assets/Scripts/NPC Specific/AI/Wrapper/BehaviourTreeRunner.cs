using System.Linq;
using AI.BehaviourTree;
using Fusion;
using UnityEngine;

public class BehaviourTreeRunner : NetworkBehaviour, ITreeRunner
{
    [SerializeField,
     Tooltip("The data for this fish")]
    private FishData fishData;
    
    [SerializeField,
     Tooltip("The behaviour tree to generate and execute")]
    private BehaviourTreeSO behaviourTreeToGenerate;

    /// <summary>
    /// The generated behaviour tree to execute on this object.
    /// </summary>
    private BehaviourTree _behaviourTreeToExecute;
    
    private bool _isInsideArea;
    private Vector3 _directionToArea;
    
    /// <summary>
    /// Whether the fish is in danger
    /// </summary>
    private bool _isInDanger;

    /// <summary>
    /// Whether the fish is locked onto prey.
    /// </summary>
    private bool _isHunting;

    private Transform _target;

    public FishData FishType => fishData;
    
    public override void Spawned()
    {
        behaviourTreeToGenerate.SetRunner(this);
        _behaviourTreeToExecute = behaviourTreeToGenerate.Construct();
    }

    private void Update()
    {
        _behaviourTreeToExecute?.Evaluate();
    }

    public void AdjustAreaCheck((bool isInside, Vector3 direction) areaCheck)
    {
        _isInsideArea = areaCheck.isInside;
        _directionToArea = areaCheck.direction;
    }

    public void AdjustHuntOrFleeTarget((Transform targetTransform, ITreeRunner targetBehaviour) targetData)
    {
        if (_isHunting || _isInDanger) return;

        _target = targetData.targetTransform;
        
        if (targetData.targetBehaviour.FishType.PredatorList.Contains(FishType))
        {
            _isInDanger = true;
            return;
        }
        
        if (FishType.PreyList.Contains(targetData.targetBehaviour.FishType))
        {
            _isHunting = true;
        }
    }
}
