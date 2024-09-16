using Fusion;
using StarterAssets;
using UnityEngine;

public class Experience : NetworkBehaviour
{
    public int experienceUntilUpgrade = 300;
    [HideInInspector] public int currentExperience;
    [SerializeField] private int experienceIncreaseOnLevelUp = 200;

    [SerializeField] private float defaultSwimSpeedAdditionOnLevelUp = 1f;
    [SerializeField] private float boostSwimSpeedIncreaseOnLevelUp = 1f;
    [SerializeField] private float attackDamageIncreaseOnLevelUp = .5f;
    [SerializeField] private float sizeIncreaseOnLevelUp = .1f;
    [SerializeField] private float suckPowerIncreaseOnLevelUp = .1f;

    private void Update()
    {
        if (currentExperience >= experienceUntilUpgrade)
        {
            GetComponent<ThirdPersonController>().transform.localScale += new Vector3(sizeIncreaseOnLevelUp, sizeIncreaseOnLevelUp, sizeIncreaseOnLevelUp);
            GetComponent<ThirdPersonController>().notMovingFOV += 1f;
            GetComponent<ThirdPersonController>().defaultSpeedFOV += 1f;
            GetComponent<ThirdPersonController>().boostSpeedFOV += 1f;
            GetComponent<ThirdPersonController>().boostSwimSpeed += defaultSwimSpeedAdditionOnLevelUp;
            GetComponent<ThirdPersonController>().defaultSwimSpeed += boostSwimSpeedIncreaseOnLevelUp;
            GetComponent<ThirdPersonController>().playerManager.attack.attackDamage += attackDamageIncreaseOnLevelUp;
            GetComponent<ThirdPersonController>().playerManager.attack.suckPower += suckPowerIncreaseOnLevelUp;
            experienceUntilUpgrade += experienceIncreaseOnLevelUp;
            currentExperience = 0;
        }
    }
}
