using System;
using System.Collections;
using Fusion;
using StarterAssets;
using TMPro;
using UnityEngine;

public class LevelUp : NetworkBehaviour
{
    [HideInInspector] public int currentLevel { get; private set; }

    [Header("Starting Values")]
    [HideInInspector] public int startingExperienceUntilUpgrade;
    [HideInInspector] public int startingExperience;
    [HideInInspector] public Vector3 startingSize;
    [HideInInspector] public float startingCameraDistance;
    [HideInInspector] public float startingBoostSwimSpeed;
    [HideInInspector] public float startingDefaultSwimSpeed;
    [HideInInspector] public float startingAttackDamage;
    [HideInInspector] public float startingSuckPower;
    [HideInInspector] public float startingAttackRange;
    [HideInInspector] public float startingHealth;

    [Header("Upgrading Values")]
    public int experienceUntilUpgrade = 300;
    [HideInInspector] public int currentExperience;
    [SerializeField] public int experienceIncreaseOnLevelUp = 200;

    [SerializeField] public float defaultSwimSpeedIncreaseOnLevelUp = 1f;
    [SerializeField] public float boostSwimSpeedIncreaseOnLevelUp = 1f;
    [SerializeField] public float attackDamageIncreaseOnLevelUp = .5f;
    [SerializeField] public float sizeIncreaseOnLevelUp = .1f;
    [SerializeField] public float suckPowerIncreaseOnLevelUp = .1f;
    [SerializeField] public float cameraDistanceIncreaseOnLevelUp = .75f;
    [SerializeField] public float attackRangeIncreaseOnLevelUp = .2f;
    [SerializeField] public float healthIncreaseOnLevelUp = 20f;
    


    private PlayerManager playerManager;

    [HideInInspector] public bool isEgg;
    [SerializeField] private GameObject eggModel;
    [SerializeField] private GameObject fishModel;
    [SerializeField] private ParticleSystem LevelUpParticleSystem;

    public Action levelUpEvent;
    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        startingExperienceUntilUpgrade = experienceUntilUpgrade;
        startingExperience = currentExperience;
        startingSize = playerManager.thirdPersonController.transform.localScale;
        startingCameraDistance = playerManager.thirdPersonController.cameraDistance;
        startingBoostSwimSpeed = playerManager.thirdPersonController.boostSwimSpeed;
        startingDefaultSwimSpeed = playerManager.thirdPersonController.defaultSwimSpeed;
        startingAttackDamage = playerManager.playerAttack.attackDamage;
        startingSuckPower = playerManager.playerAttack.suckInDamage;
        startingAttackRange = playerManager.playerAttack.attackRange;
        startingHealth = playerManager.healthManager.maxHealth;

        Restart();
    }
    public void Restart()
    {
        currentExperience = startingExperience;
        experienceUntilUpgrade = startingExperienceUntilUpgrade;
        currentLevel = 0;
        isEgg = true;
        eggModel.SetActive(true);
        fishModel.SetActive(false);
    }
    public void CheckLevelUp()
    {
        if (currentExperience >= experienceUntilUpgrade)
        {
            LevelUpParticleSystem.Play();
            if (isEgg)
            {
                EvolutionIntoFishRpc();
            }

            playerManager.thirdPersonController.transform.localScale += new Vector3(sizeIncreaseOnLevelUp, sizeIncreaseOnLevelUp, sizeIncreaseOnLevelUp);
            playerManager.thirdPersonController.cameraDistance += cameraDistanceIncreaseOnLevelUp;
            playerManager.thirdPersonController.boostSwimSpeed += defaultSwimSpeedIncreaseOnLevelUp;
            playerManager.thirdPersonController.defaultSwimSpeed += boostSwimSpeedIncreaseOnLevelUp;
            playerManager.playerAttack.attackDamage += attackDamageIncreaseOnLevelUp;
            playerManager.playerAttack.suckInDamage += suckPowerIncreaseOnLevelUp;
            playerManager.playerAttack.attackRange += attackRangeIncreaseOnLevelUp;
            playerManager.healthManager.maxHealth += healthIncreaseOnLevelUp;
            experienceUntilUpgrade += experienceIncreaseOnLevelUp;
            currentExperience = 0;
            AudioManager.Instance.Play("levelUp");
            currentLevel++;
            levelUpEvent?.Invoke();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void EvolutionIntoFishRpc()
    {
        isEgg = false;
        eggModel.SetActive(false);
        fishModel.SetActive(true);
        playerManager.transform.position = playerManager.transform.position + new Vector3(0,0.2f,0);
    }
}
