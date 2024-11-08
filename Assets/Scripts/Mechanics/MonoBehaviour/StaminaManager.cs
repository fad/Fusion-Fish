using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class StaminaManager : MonoBehaviour, IStaminaManager, IInitialisable
{
    [Header("Settings")]
    [SerializeField, Tooltip("The data for this fish")]
    private FishData fishData;
    
    [SerializeField, Tooltip("The time it takes to start regenerating stamina")]
    private float timeToStartRegenerate = 3f;
    
    [SerializeField, Tooltip("The cooldown for decreasing stamina")]
    private float decreaseCooldown = 1f;
    
    private short _currentStamina;
    private bool _isRegenerating;
    private float _timePassedAfterLastDecrease;
    private float _currentDecreaseCooldownTimer;

    public short CurrentStamina => _currentStamina;

    private void OnEnable()
    {
        _currentStamina = fishData.MaxStamina;
    }

    private void Update()
    {
        Regenerate();
    }

    public void Init(string fishDataName)
    {
        FishSpawnHandler.Instance.FishDataNameDictionary.TryGetValue(fishDataName, out fishData);
    }

    public void Decrease()
    {
        if (_currentStamina <= 0) return;
        
        if(_currentDecreaseCooldownTimer < decreaseCooldown)
        {
            _currentDecreaseCooldownTimer += Time.deltaTime;
            return;
        }
        
        if (_isRegenerating)
        {
            _isRegenerating = false;
            StopCoroutine(RegenerateStamina());
        }
        
        _currentStamina -= fishData.StaminaDecreaseRate;
        _currentStamina = (short) Mathf.Clamp(_currentStamina, 0, fishData.MaxStamina);
        _timePassedAfterLastDecrease = 0f; // Reset the timer after decreasing stamina
        _currentDecreaseCooldownTimer = 0f;
    }

    public void Regenerate()
    {
        if(_currentStamina == fishData.MaxStamina || _isRegenerating) return;
        
        _timePassedAfterLastDecrease += Time.deltaTime;
        
        if (_timePassedAfterLastDecrease >= timeToStartRegenerate && !_isRegenerating)
        {
            _isRegenerating = true;
            StartCoroutine(RegenerateStamina());
        }
    }

    private IEnumerator RegenerateStamina()
    {
        _timePassedAfterLastDecrease = 0f;

        while (_currentStamina < fishData.MaxStamina && _isRegenerating)
        {
            _currentStamina += fishData.StaminaRegenRate;
            yield return new WaitForSeconds(1f);
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(StaminaManager))]
public class StaminaManager_CustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        StaminaManager staminaManager = (StaminaManager) target;
        
        EditorGUILayout.Space(15f);

        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("Current stamina: ");
        EditorGUILayout.LabelField(staminaManager.CurrentStamina.ToString());
        
        EditorGUILayout.EndHorizontal();
        
    }
}

#endif