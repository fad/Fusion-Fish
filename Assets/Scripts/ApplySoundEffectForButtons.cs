using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ApplySoundEffectForButtons : MonoBehaviour
{
    //Made this method because it is way too tedious applying all the sounds manually
    private void Start()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        
        foreach (var b in buttons)
        {
            b.onClick.AddListener(() => AudioManager.Instance.Play("click"));
            EventTrigger trigger = b.gameObject.AddComponent<EventTrigger>();
            var pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((e) => AudioManager.Instance.Play("hover"));
            trigger.triggers.Add(pointerEnter);
        }
    }
}
