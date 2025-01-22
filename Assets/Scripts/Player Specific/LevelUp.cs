using System;
using System.Collections;
using Fusion;
using StarterAssets;
using TMPro;
using UnityEngine;

public class LevelUp : NetworkBehaviour
{
    [Networked][HideInInspector] public int currentLevel { get; private set; }
    [HideInInspector] public PlayerFishData currentLevelFishData { get; private set; }

    private int currentExperience;
    private PlayerManager playerManager;

    [HideInInspector] public bool isEgg;
    [SerializeField] private GameObject eggModel;
    [SerializeField] private GameObject fishModel;
    [SerializeField] private ParticleSystem LevelUpParticleSystem;
    [SerializeField] private PlayerFishData[] LevelsFishData;

    public Action levelUpEvent;
    public Action<int> AddExperienceEvent;
    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();

        if (currentLevel > 0)
            EvolutionIntoFishRpc();
        else
            Restart();
    }
    public void Restart()
    {
        currentLevel = 0;
        currentLevelFishData = LevelsFishData[currentLevel];
        UpdateFishData(currentLevelFishData);
        isEgg = true;
        eggModel.SetActive(true);
        fishModel.SetActive(false);
    }

    public int GetExperience()
    {
        return currentExperience;
    }
    public void AddExperience(int experience)
    {
        if (!HasStateAuthority) return;
        
        currentExperience += experience;
        AddExperienceEvent?.Invoke(experience);
        CheckLevelUpRpc();
    }    
    

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void CheckLevelUpRpc()
    {
        if(!currentLevelFishData) return;
        
        if (currentExperience < currentLevelFishData.ExperienceUntilUpgrade) return;
        
        currentLevel++;
        currentLevelFishData = LevelsFishData[currentLevel];

        LevelUpParticleSystem.Play();
        if (isEgg)
        {
            EvolutionIntoFishRpc();
        }
            
        UpdateFishData(currentLevelFishData);
        currentExperience = 0;
        AudioManager.Instance.Play("levelUp");
        levelUpEvent?.Invoke();
    }

    private void UpdateFishData(PlayerFishData fishData)
    {
        playerManager.satietyManager.UpdateSatietyData(fishData.MaxSatiety,fishData.SatietyDecreaseRate);
        playerManager.entityDataContainer.FishDataUpdate(fishData);
        playerManager.spawnGibsOnDestroy.FishDataUpdate(fishData);
        playerManager.thirdPersonController.transform.localScale = new Vector3(fishData.Scale, fishData.Scale, fishData.Scale);
        playerManager.thirdPersonController.boostSwimSpeed = fishData.FastSpeed;
        playerManager.thirdPersonController.defaultSwimSpeed = fishData.WanderSpeed;
        playerManager.thirdPersonController.boostReloadSpeed = fishData.StaminaRegenRate;
        playerManager.healthManager.maxHealth = fishData.MaxHealth;
        playerManager.healthManager.recoveryHealthInSecond = fishData.RecoveryHealthInSecond;
        playerManager.healthManager.timeToStartRecoveryHealth = fishData.TimeToStartRecoveryHealth;
        playerManager.healthManager.XPValue = fishData.XPValue;
        playerManager.playerAttack.attackDamage = fishData.AttackValue;
        playerManager.playerAttack.biteStunDuration = fishData.BiteStunDuration;
        playerManager.playerAttack.stunChance = fishData.StunChance;
        playerManager.playerAttack.attackRange = fishData.AttackRange;
        playerManager.thirdPersonController.cameraDistance = fishData.Scale * 5 / 0.25f;
        playerManager.playerAttack.suckInDamage = 10;
    }
    public int GetLevel()
    {
        if (HasStateAuthority)
            return currentLevel;
        else
            return 0;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void EvolutionIntoFishRpc()
    {
        isEgg = false;
        eggModel.SetActive(false);
        fishModel.SetActive(true);
        playerManager.transform.position = playerManager.transform.position + new Vector3(0, 0.2f, 0);
    }
}
