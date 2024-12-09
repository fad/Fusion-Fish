using UnityEngine;

/// <summary>
/// Helper class to store the data for the player. Used to make NPCs hunt or flee from the player.
/// </summary>
public class EntityDataContainer : MonoBehaviour, IEntity
{
    [Header("Settings")]
    [SerializeField, Tooltip("The data for this fish")]
    private FishData fishData;

    public FishData FishType => fishData;
}
