using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SetUIActivationState : MonoBehaviour
{
    [SerializeField] private GameObject[] uIObjects;
    private bool uiObjectsActive;
    [HideInInspector] public bool pressedActivationUIMultiplayerButton;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private CanvasGroup LoadPanel;

    private void Start()
    {
        LoadPanel.gameObject.SetActive(true);

        foreach (var uiObject in uIObjects)
        {
            uiObject.SetActive(false);
        }
    }
    public void DeactiveLoadPanel()
    {
        StartCoroutine(DeactiveLoadPanelCor());
    }

    private IEnumerator DeactiveLoadPanelCor()
    {
        float timeStartedLerping = Time.time;
        bool reachedEnd = false;

        while (!reachedEnd)
        {
            float timeSinceStarted = Time.time - timeStartedLerping;
            float percentageComplete = timeSinceStarted / 1.5f;

            LoadPanel.alpha = Mathf.Lerp(1, 0, percentageComplete);

            if (percentageComplete >= 1.0f)
            {
                reachedEnd = true;
            }

            yield return new WaitForEndOfFrame();
        }
        LoadPanel.gameObject.SetActive(false);
    }
    public void SetActiveUIObjects()
    {
        if (uiObjectsActive)
        {
            pressedActivationUIMultiplayerButton = true;

            foreach (var uiObject in uIObjects)
            {
                inputField.ReleaseSelection();
                var eventSystem = EventSystem.current;
                if (!eventSystem.alreadySelecting) 
                    eventSystem.SetSelectedGameObject (null);                
                
                uiObject.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            
            uiObjectsActive = false;
        }
        else
        {
            pressedActivationUIMultiplayerButton = true;

            foreach (var uiObject in uIObjects)
            {
                uiObject.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            
            uiObjectsActive = true;
        }
    }
}
