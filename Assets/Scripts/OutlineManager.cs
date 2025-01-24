using UnityEngine;

public class OutlineManager : MonoBehaviour
{
    [SerializeField] 
    private Outline objectOutline;

    private void OnEnable()
    {
        if (!objectOutline)
        {
            Debug.LogWarning($"OutlineManager: objectOutline is not set in {gameObject.name}!");
            return;
        }       
        
        objectOutline.enabled = false;
    }

    public void ShouldOutline(bool shouldOutline)
    {
        if(!objectOutline) return;
        
        objectOutline.enabled = shouldOutline;
    }
}
