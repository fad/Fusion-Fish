using System.Collections;
using Fusion;
using UnityEngine;

public class StaminaManager : NetworkBehaviour, IStaminaManager, IInitialisable
{
    [Header("Settings")]
    [SerializeField, Tooltip("The data for this fish")]
    private FishData fishData;
    
    [SerializeField, Tooltip("The time it takes to start regenerating stamina")]
    private float timeToStartRegenerate = 3f;
    
    [SerializeField, Tooltip("The cooldown for decreasing stamina")]
    private float decreaseCooldown = 1f;
    [Networked,HideInInspector] public short CurrentStamina { get; private set; }
    
    [Networked] private bool IsRegenerating { get; set; }
    [Networked] private float TimePassedAfterLastDecrease { get; set; }
    [Networked] private float CurrentDecreaseCooldownTimer { get; set; }
    
    [Networked] private bool HasFreshlySpawned { get; set; }


    public override void Spawned()
    {
        if (!HasFreshlySpawned) return;
        
        CurrentStamina = fishData.MaxStamina;
        HasFreshlySpawned = false;
    }

    public override void FixedUpdateNetwork()
    {
        Regenerate();
    }

    public void Init(string fishDataName)
    {
        FishSpawnHandler.Instance.FishDataNameDictionary.TryGetValue(fishDataName, out fishData);
    }

    public void Decrease()
    {
        if (CurrentStamina <= 0) return;
        
        if(CurrentDecreaseCooldownTimer < decreaseCooldown)
        {
            CurrentDecreaseCooldownTimer += Time.deltaTime;
            return;
        }
        
        if (IsRegenerating)
        {
            IsRegenerating = false;
            StopCoroutine(RegenerateStamina());
        }
        
        CurrentStamina -= fishData.StaminaDecreaseRate;
        CurrentStamina = (short) Mathf.Clamp(CurrentStamina, 0, fishData.MaxStamina);
        TimePassedAfterLastDecrease = 0f; // Reset the timer after decreasing stamina
        CurrentDecreaseCooldownTimer = 0f;
    }

    public void Regenerate()
    {
        if(CurrentStamina == fishData.MaxStamina || IsRegenerating) return;
        
        TimePassedAfterLastDecrease += Time.deltaTime;
        
        if (TimePassedAfterLastDecrease >= timeToStartRegenerate && !IsRegenerating)
        {
            IsRegenerating = true;
            StartCoroutine(RegenerateStamina());
        }
    }

    private IEnumerator RegenerateStamina()
    {
        TimePassedAfterLastDecrease = 0f;

        while (CurrentStamina < fishData.MaxStamina && IsRegenerating)
        {
            CurrentStamina += fishData.StaminaRegenRate;
            yield return new WaitForSeconds(1f);
        }
    }
}