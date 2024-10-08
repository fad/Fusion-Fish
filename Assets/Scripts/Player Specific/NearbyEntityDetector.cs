using UnityEngine;

public class NearbyEntityDetector : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField] private LayerMask layerMaskToCheck;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == gameObject) return;
        if(!IsEntity(other.gameObject)) return;
        
        DealWithHealthUI(other.gameObject, true);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == gameObject) return;
        if(!IsEntity(other.gameObject)) return;
        
        DealWithHealthUI(other.gameObject, false);
    }

    private void DealWithHealthUI(GameObject entity, bool shouldShowHealthUI)
    {
        bool hasHealthViewModel = entity.TryGetComponent(out HealthViewModel healthViewModel);
        if(!hasHealthViewModel) return;

        
        healthViewModel.AdjustHealthBarVisibility(shouldShowHealthUI);
    }
    
    private bool IsEntity(GameObject obj)
    {
        return (layerMaskToCheck & (1<<obj.layer)) != 0;
    }
    
    
}
