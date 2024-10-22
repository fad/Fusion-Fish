using System.Collections;
using UnityEngine;

public class StaminaManager : MonoBehaviour, IStaminaManager
{
    [Header("Settings")]
    [SerializeField, Tooltip("The data for this fish")]
    private FishData fishData;
    
    [SerializeField, Tooltip("The time it takes to start regenerating stamina")]
    private float timeToStartRegenerate = 3f;
    
    private short _currentStamina;
    private bool _isRegenerating;
    private float _timePassedAfterLastDecrease;

    public short CurrentStamina => _currentStamina;

    private void OnEnable()
    {
        _currentStamina = fishData.MaxStamina;
    }

    private void Update()
    {
        Regenerate();
    }

    public void Decrease()
    {
        if (_isRegenerating)
        {
            StopCoroutine(RegenerateStamina());
        }
        
        _currentStamina -= fishData.StaminaDecreaseRate;
        _currentStamina = (short) Mathf.Clamp(_currentStamina, 0, fishData.MaxStamina);
        _timePassedAfterLastDecrease = 0f; // Reset the timer after decreasing stamina
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

        while (_currentStamina < fishData.MaxStamina)
        {
            _currentStamina += fishData.StaminaRegenRate;
            yield return new WaitForSeconds(1f);
        }
    }
}
