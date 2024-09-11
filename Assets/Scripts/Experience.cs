using StarterAssets;
using UnityEngine;
using Fusion;

public class Experience : NetworkBehaviour
{
    public int experienceUntilUpgrade = 300;
    [HideInInspector] public int currentExperience;

    [SerializeField] private float defaultSwimSpeedIncreaseOnLevelUp = 1f;
    [SerializeField] private float boostSwimSpeedIncreaseOnLevelUp = 1f;
    [SerializeField] private float attackDamageIncreaseOnLevelUp = .5f;
    [SerializeField] private float sizeIncreaseOnLevelUp = .25f;

    private void Update()
    {
        if (currentExperience >= experienceUntilUpgrade)
        {
            GetComponent<ThirdPersonController>().transform.localScale += new Vector3(sizeIncreaseOnLevelUp, sizeIncreaseOnLevelUp, sizeIncreaseOnLevelUp);
            GetComponent<ThirdPersonController>().notMovingFOV += .5f;
            GetComponent<ThirdPersonController>().defaultSpeedFOV += .5f;
            GetComponent<ThirdPersonController>().boostSpeedFOV += .5f;
            GetComponent<ThirdPersonController>().boostSwimSpeed += defaultSwimSpeedIncreaseOnLevelUp;
            GetComponent<ThirdPersonController>().defaultSwimSpeed += boostSwimSpeedIncreaseOnLevelUp;
            GetComponent<ThirdPersonController>().playerManager.attack.attackDamage += attackDamageIncreaseOnLevelUp;
            currentExperience = 0;
        }
    }
}
