using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenURLButton : MonoBehaviour
{
    [SerializeField] private URLData urlData;

    public virtual void OpenLink()
    {
        if (!string.IsNullOrEmpty(urlData.url))
        {
            Application.OpenURL(urlData.url);
        }
        else
        {
            Debug.LogWarning("URL not set!");
        }
    }
}
