    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerFishData", menuName = "Data/New Player Fish Data", order = 0)]
public class PlayerFishData : FishData
{
    [Header("Player Settings")]

    [SerializeField, Tooltip("The amount of experience required to level up.")]
    private int experienceUntilUpgrade = 300;

    [SerializeField, Tooltip("The amount of experience required to level up.")]
    private int maxSatiety = 120;  

    [SerializeField, Tooltip("The amount of experience required to level up.")]
    private int satietyDecreaseRate = 1;   

    public int ExperienceUntilUpgrade => experienceUntilUpgrade;
    public int MaxSatiety => maxSatiety;
    public int SatietyDecreaseRate => satietyDecreaseRate;
}
