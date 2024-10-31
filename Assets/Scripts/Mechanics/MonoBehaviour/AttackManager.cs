using System;
using UnityEngine;

public class AttackManager : MonoBehaviour, IAttackManager
{
    [SerializeField]
    private FishData fishData;

    private ITreeRunner _treeRunnerForThisFish;
    private float _currentAttackCooldown = 0f;

    private void Start()
    {
        TryGetComponent(out _treeRunnerForThisFish);

        if (_treeRunnerForThisFish == null)
        {
            Debug.LogError($"No <color=#00cec9>ITreeRunner</color> component found on object: {gameObject.name}.");
        }
    }

    private void Update()
    {
        if (_currentAttackCooldown <= 0f) return;
        
        _currentAttackCooldown -= Time.deltaTime;
    }


    public void Attack(float damageValue, Transform target)
    {
        if(_currentAttackCooldown > 0f) return;
        
        target.TryGetComponent(out IHealthManager healthManager);
        healthManager?.Damage(damageValue);

        if (healthManager is { Died: true }) return;

        target.TryGetComponent(out ITreeRunner treeRunner);
        treeRunner?.AdjustHuntOrFleeTarget((transform, _treeRunnerForThisFish));
        
        _currentAttackCooldown = fishData.AttackCooldown;
    }
}
