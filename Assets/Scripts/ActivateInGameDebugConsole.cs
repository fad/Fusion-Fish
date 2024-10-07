using UnityEngine;
using IngameDebugConsole;

public class ActivateInGameDebugConsole : MonoBehaviour
{
    [SerializeField] private KeyCode keyX = KeyCode.X;
    [SerializeField] private KeyCode keyC = KeyCode.C;
    
    private DebugLogManager debugConsole;
    private const float CheckInterval = 0.1f;
    private float nextCheckTime;

    private void Start() => debugConsole = gameObject.GetComponent<DebugLogManager>();

    void Update()
    {
        if (!(Time.time >= nextCheckTime)) 
            return;
        
        var activate = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKey(keyX) && Input.GetKey(keyC);

        if (activate)
        {
            if (debugConsole.IsLogWindowVisible)
                debugConsole.HideLogWindow();
            else
                debugConsole.ShowLogWindow();
        }
        
        nextCheckTime = Time.time + CheckInterval;
    }
}
