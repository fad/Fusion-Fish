using UnityEngine;

public class NPCSetup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("The data for this NPC")]
    private FishData fishData;

    [SerializeField, Tooltip("The mesh to use for the NPC")]
    private GameObject model; 
    
    public FishData FishData => fishData;
    public GameObject Model => model;
    
    public bool CheckFishData()
    {
        return fishData;
    }
}
