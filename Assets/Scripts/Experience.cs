using StarterAssets;
using UnityEngine;

public class Experience : MonoBehaviour
{
    public int experienceUntilUpgrade = 300;
    private ThirdPersonController thirdPersonController;
    [HideInInspector] public int currentExperience;

    private void Start()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
    }

    private void Update()
    {
        if (currentExperience > experienceUntilUpgrade)
        {
            thirdPersonController.transform.localScale += new Vector3(0.25f, 0.25f, 0.25f);
            currentExperience = 0;
        }
    }
}
