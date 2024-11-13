using Fusion;
using UnityEngine;

public class AttackManager : NetworkBehaviour, IAttackManager, IInitialisable
{

    [Header("Setup Settings")]
    
    [SerializeField, Tooltip("The data for this fish")]
    private FishData fishData;
    [Networked] private float CurrentAttackCooldown { get; set; }

    private IEntity _correspondingEntity;
    private Animator _animator;
    
    private static readonly int AttackTrigger = Animator.StringToHash("attack");

    public void Init(string fishDataName)
    {
        _correspondingEntity = transform.parent.GetComponentInChildren<IEntity>();

        if (_correspondingEntity == null)
        {
            Debug.LogError($"No <color=#00cec9>ITreeRunner</color> component found on object: {gameObject.name}.");
        }
        
        FishSpawnHandler.Instance.FishDataNameDictionary.TryGetValue(fishDataName, out fishData);
        
        if (!fishData)
        {
            Debug.LogError($"No <color=#00cec9>FishData</color> found with name: {fishDataName}.");
        }
        
        _animator = GetComponentInChildren<Animator>();
    }

    public override void FixedUpdateNetwork()
    {
        if (CurrentAttackCooldown <= 0f) return;
        
        CurrentAttackCooldown -= Time.deltaTime;
    }
    
    


    public void Attack(float damageValue, Transform target)
    {
        if(CurrentAttackCooldown > 0f) return;
        
        target.TryGetComponent(out IHealthManager healthManager);
        healthManager?.Damage(damageValue);

        if (_animator)
        {
            _animator.SetTrigger(AttackTrigger);
        }

        if (healthManager is { Died: true }) return;

        target.TryGetComponent(out ITreeRunner treeRunner);
        treeRunner?.AdjustHuntOrFleeTarget((transform, _correspondingEntity));
        
        CurrentAttackCooldown = fishData.AttackCooldown;
    }
}
