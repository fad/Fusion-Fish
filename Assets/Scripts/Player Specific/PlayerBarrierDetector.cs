using StarterAssets;
using UnityEngine;

public class PlayerBarrierDetector : MonoBehaviour
{
    private ThirdPersonController thirdPersonController;
    private float distance = 2;
    [SerializeField] private LayerMask layerMask;
    private void Start()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
    }
    
    private void Update()
    {
        if(!thirdPersonController || !thirdPersonController.playerVisual) return;
        
        RaycastHit hit;
        Transform playerVisual = thirdPersonController.playerVisual.transform;
        if(Physics.Raycast(playerVisual.position, -playerVisual.forward, out hit, distance, layerMask))
        {
            thirdPersonController.PushAway(Vector3.Normalize(playerVisual.position - hit.point));
        }
    }
   
}
