using UnityEngine;

public class SetUIActivationState : MonoBehaviour
{
    [SerializeField] private GameObject[] uIObjects;
    private bool uiObjectsActive;
    [HideInInspector] public bool pressedActivationUIMultiplayerButton;

    private void Start()
    {
        foreach (var uiObject in uIObjects)
        {
            uiObject.SetActive(false);
        }
    }

    public void SetActiveUIObjects()
    {
        if (uiObjectsActive)
        {
            pressedActivationUIMultiplayerButton = true;

            foreach (var uiObject in uIObjects)
            {
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
