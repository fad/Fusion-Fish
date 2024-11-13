using UnityEngine;

public class FishDataSetupManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("The data for this fish")]
    private FishData fishData;
    
    public void SetupFishData()
    {
        if (!fishData)
        {
            Debug.LogError($"<color=#f0932b>[{name}]</color>: Cannot setup fish data, no fish data found! Please set it up first!");
        }

        var initialisables = GetComponentsInChildren<IInitialisable>();
        
        foreach (var initialisable in initialisables)
        {
            initialisable.Init(fishData.name);
        }
    }
}


