using UnityEngine;

public class NPCSetup : MonoBehaviour
{
    [Header("Meta Settings")]
    [SerializeField, Tooltip("The data for this NPC")]
    private FishData fishData;

    [SerializeField, Tooltip("The mesh to use for the NPC")]
    private GameObject model;
    
    [Header("Detection Settings")]
    [SerializeField, Tooltip("The detection radius")]
    private float detectionRadius = 10f;
    
    
    public FishData FishData => fishData;
    public GameObject Model => model;
    public float DetectionRadius => detectionRadius;
    
    public bool CheckFishData()
    {
        return fishData;
    }
}
