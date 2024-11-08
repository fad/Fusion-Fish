using System;
using UnityEngine;

public class AttackManager : MonoBehaviour, IAttackManager, IInitialisable
{
    [Header("Setup Settings")]
    
    [SerializeField, Tooltip("The data for this fish")]
    private FishData fishData;

    private IEntity _correspondingEntity;
    private float _currentAttackCooldown = 0f;

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
        treeRunner?.AdjustHuntOrFleeTarget((transform, _correspondingEntity));
        
        _currentAttackCooldown = fishData.AttackCooldown;
    }
}
