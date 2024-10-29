using UnityEngine;

public class AttackManager : MonoBehaviour, IAttackManager
{
        private ITreeRunner _treeRunnerForThisFish;
        
        private void Start()
        {
                TryGetComponent(out _treeRunnerForThisFish);
                
                if (_treeRunnerForThisFish == null)
                {
                        Debug.LogError($"No <color=#00cec9>ITreeRunner</color> component found on object: {gameObject.name}.");
                }
        }
        
        
        public void Attack(float damageValue, Transform target)
        {
                target.TryGetComponent(out IHealthManager healthManager);
                healthManager?.Damage(damageValue);
                
                target.TryGetComponent(out ITreeRunner treeRunner);
                treeRunner?.AdjustHuntOrFleeTarget((target, _treeRunnerForThisFish));
        }
}
