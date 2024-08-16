using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ApplySoundEffectForButtons : MonoBehaviour
{
    private void Start()
    {
        Button[] buttons = FindObjectsOfType<Button>(true); // parameter makes it include inactive UI elements with buttons
        
        foreach (var b in buttons)
        {
            b.onClick.AddListener(ButtonSound);
            EventTrigger trigger = b.gameObject.AddComponent<EventTrigger>();
            var pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((e) => AudioManager.Instance.Play("hover"));
            trigger.triggers.Add(pointerEnter);
        }
    }
    
    public void ButtonSound()
    {
       AudioManager.Instance.Play("click");
    }
}
