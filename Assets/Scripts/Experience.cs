using StarterAssets;
using UnityEngine;

public class Experience : MonoBehaviour
{
    public int experienceUntilUpgrade = 300;
    [HideInInspector] public int currentExperience;

    private void Update()
    {
        if (currentExperience >= experienceUntilUpgrade)
        {
            GetComponent<ThirdPersonController>().transform.localScale += new Vector3(0.25f, 0.25f, 0.25f);
            currentExperience = 0;
        }
    }
}
