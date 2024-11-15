using Fusion;
using UnityEngine;

public class AttackManager : NetworkBehaviour, IAttackManager, IInitialisable
{

    [Header("Setup Settings")]
    
    [SerializeField, Tooltip("The data for this fish")]
    private FishData fishData;
    [Networked] private float CurrentAttackCooldown { get; set; }

    private INPC _correspondingNPC;
    private Animator _animator;
    
    private Transform _currentTarget;
    private IHealthManager _currentTargetHealthManager;
    private ITreeRunner _currentTargetTreeRunner;
    
    private static readonly int AttackTrigger = Animator.StringToHash("attack");

    public void Init(string fishDataName)
    {
        _correspondingNPC = transform.parent.GetComponent<INPC>();

        if (_correspondingNPC == null)
        {
            Debug.LogError($"No <color=#00cec9>ITreeRunner</color> component found on object: {gameObject.name}.");
        }
        
        _correspondingNPC.OnTargetChanged += ChangeTarget;
        
        FishSpawnHandler.Instance.FishDataNameDictionary.TryGetValue(fishDataName, out fishData);
        
        if (!fishData)
        {
            Debug.LogError($"No <color=#00cec9>FishData</color> found with name: {fishDataName}.");
        }
        
        _animator = GetComponentInChildren<Animator>();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _correspondingNPC.OnTargetChanged -= ChangeTarget;
    }

    public override void FixedUpdateNetwork()
    {
        if (CurrentAttackCooldown <= 0f) return;
        
        CurrentAttackCooldown -= Time.deltaTime;
    }
    
    


    public void Attack(float damageValue, Transform target)
    {
        if(CurrentAttackCooldown > 0f) return;
        if(!_currentTarget) return;
        
        _currentTargetHealthManager?.Damage(damageValue);

        if (_animator)
        {
            _animator.SetTrigger(AttackTrigger);
        }
        
        CurrentAttackCooldown = fishData.AttackCooldown;

        if (_currentTargetHealthManager is { Died: true }) return;

        _currentTargetTreeRunner?.AdjustHuntOrFleeTarget((transform, _correspondingNPC));
        
    }

    private void ChangeTarget(Transform newTarget)
    {
        if (!newTarget)
        {
            if(_currentTargetHealthManager != null)
                _currentTargetHealthManager.OnDeath -= OnTargetDeath;
            
            _currentTargetHealthManager = null;
            _currentTarget = null;
            return;
        }
        
        _currentTarget = newTarget;
        
        _currentTarget.TryGetComponent(out _currentTargetHealthManager);
        _currentTarget.TryGetComponent(out _currentTargetTreeRunner);

        _currentTargetHealthManager.OnDeath += OnTargetDeath;
    }

    private void OnTargetDeath()
    {
        ChangeTarget(null);
    }
}
