using UnityEngine;

public class OutlineManager : MonoBehaviour
{
    [SerializeField] 
    private Outline objectOutline;
    
    public void ShouldOutline(bool shouldOutline)
    {
        objectOutline.enabled = shouldOutline;
    }
}
