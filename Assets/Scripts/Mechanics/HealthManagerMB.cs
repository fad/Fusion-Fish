using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HealthManagerMB : MonoBehaviour, IHealthManager
{
    [SerializeField]
    private FishData fishData;
    
    private float _currentHealth;
    
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;
    
    public float CurrentHealth => _currentHealth;

    private void Start()
    {
        _currentHealth = fishData.MaxHealth;
    }

    private void OnDestroy()
    {
        OnDeath?.Invoke();
    }

    public void Damage(float amount)
    {
        _currentHealth -= amount;
        OnHealthChanged?.Invoke(_currentHealth);

        Die();
    }

    public void Heal(float amount)
    {
        throw new NotImplementedException();
    }

    private void Die()
    {
        if (_currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(HealthManagerMB))]
public class HealthManagerMbCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        HealthManagerMB HM = (HealthManagerMB) target;
        
        EditorGUILayout.Space(15f);

        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("Current health: ");
        EditorGUILayout.LabelField(HM.CurrentHealth.ToString());
        
        EditorGUILayout.EndHorizontal();
        
    }
}

#endif