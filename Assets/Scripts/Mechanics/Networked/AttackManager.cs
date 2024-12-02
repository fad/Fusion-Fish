using Fusion;
using StarterAssets;
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
    private ThirdPersonController _thirdPersonController;
    
    private static readonly int AttackTrigger = Animator.StringToHash("attack");

    public void Init(string fishDataName)
    {
        
    }

    public override void Spawned()
    {
        _correspondingNPC = transform.GetComponentInParent<INPC>();

        if (_correspondingNPC == null)
        {
            Debug.LogError($"No <color=#00cec9>INPC</color> component found on object: {gameObject.name}.");
            return;
        }
        
        _correspondingNPC.OnTargetChanged += ChangeTarget;
        
        
        if (!fishData)
        {
            FishSpawnHandler.Instance.FishDataNameDictionary.TryGetValue(_correspondingNPC.FishType.name, out fishData);
            if (!fishData)
            {
                Debug.LogError($"No <color=#00cec9>FishData</color> found with name: {_correspondingNPC.FishType.name}.");
                return;
            }
        }
        
        _animator = GetComponentInChildren<Animator>();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (hasState && _correspondingNPC!=null)
        {
           _correspondingNPC.OnTargetChanged -= ChangeTarget;
        }
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
        
        if (_animator)
        {
            _animator.SetTrigger(AttackTrigger);
        }
        
        CurrentAttackCooldown = fishData.AttackCooldown;
        _currentTargetHealthManager?.Damage(damageValue);

        if (_currentTargetHealthManager is { Died: true }) return;

        _currentTargetTreeRunner?.AdjustHuntOrFleeTarget((transform, _correspondingNPC));
        _thirdPersonController?.GraspedRpc(transform.GetComponent<NetworkTransform>());
    }

    private void ChangeTarget(Transform newTarget)
    {
        if (_currentTargetHealthManager != null)
        {
            _currentTargetHealthManager.OnDeath -= OnTargetDeath;
        }

        if (newTarget == null)
        {
            _currentTargetHealthManager = null;
            _currentTarget = null;
            return;
        }

        _currentTarget = newTarget;

        if (_currentTarget.TryGetComponent(out _currentTargetHealthManager))
        {
            _currentTargetHealthManager.OnDeath += OnTargetDeath;
        }

        _currentTarget.TryGetComponent(out _currentTargetTreeRunner);
        _currentTarget.TryGetComponent(out _thirdPersonController);
        
    }

    private void OnTargetDeath()
    {
        ChangeTarget(null);
    }
}
