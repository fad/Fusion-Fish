using UnityEngine;
using UnityEngine.UI;

public class ApplySoundEffectForButtons : MonoBehaviour
{
    private void Start()
    {
       var buttons = FindObjectsOfType<Button>(true); // parameter makes it include inactive UI elements with buttons
       foreach (var b in buttons)
       {
           b.onClick.AddListener(ButtonSound);
       }
    }
    
    public void ButtonSound()
    {
       AudioManager.Instance.Play("click");
    }
}
