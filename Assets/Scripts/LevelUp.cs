using Fusion;
using StarterAssets;
using UnityEngine;

public class LevelUp : NetworkBehaviour
{
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

    private void Start()
    {
        startingExperienceUntilUpgrade = experienceUntilUpgrade;
        startingExperience = currentExperience;
        startingSize = GetComponent<ThirdPersonController>().transform.localScale;
        startingCameraDistance = GetComponent<ThirdPersonController>().cameraDistance;
        startingBoostSwimSpeed = GetComponent<ThirdPersonController>().boostSwimSpeed;
        startingDefaultSwimSpeed = GetComponent<ThirdPersonController>().defaultSwimSpeed;
        startingAttackDamage = GetComponent<ThirdPersonController>().playerManager.attack.attackDamage;
        startingSuckPower = GetComponent<ThirdPersonController>().playerManager.attack.suckPower;
        startingAttackRange = GetComponent<ThirdPersonController>().playerManager.attack.attackRange;
    }

    private void Update()
    {
        if (currentExperience >= experienceUntilUpgrade)
        {
            GetComponent<ThirdPersonController>().transform.localScale += new Vector3(sizeIncreaseOnLevelUp, sizeIncreaseOnLevelUp, sizeIncreaseOnLevelUp);
            GetComponent<ThirdPersonController>().cameraDistance += cameraDistanceIncreaseOnLevelUp;
            GetComponent<ThirdPersonController>().boostSwimSpeed += defaultSwimSpeedIncreaseOnLevelUp;
            GetComponent<ThirdPersonController>().defaultSwimSpeed += boostSwimSpeedIncreaseOnLevelUp;
            GetComponent<ThirdPersonController>().playerManager.attack.attackDamage += attackDamageIncreaseOnLevelUp;
            GetComponent<ThirdPersonController>().playerManager.attack.suckPower += suckPowerIncreaseOnLevelUp;
            GetComponent<ThirdPersonController>().playerManager.attack.attackRange += attackRangeIncreaseOnLevelUp;
            experienceUntilUpgrade += experienceIncreaseOnLevelUp;
            currentExperience = 0;
        }
    }
}
