using UnityEngine;

public class OutlineManager : MonoBehaviour
{
    [SerializeField] 
    private Outline objectOutline;
    
    public void ShouldOutline(bool shouldOutline)
    {
        if(!objectOutline) return;
        
        objectOutline.enabled = shouldOutline;
    }
}
