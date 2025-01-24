using UnityEngine;
using UnityEngine.Events;

public class OnDisableEventHandler : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onPlayerVisualsDisabled;
    
    private void OnDisable()
    {
        onPlayerVisualsDisabled?.Invoke();
    }
}
