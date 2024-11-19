using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MoveExperience : MonoBehaviour
{
    private Vector3 targetPosition;    
    private TextMeshProUGUI experienceText;
    [SerializeField]private float speed;
    public void MoveToPosition(Vector3 TargetPosition,int Experience)
    {
        targetPosition = TargetPosition;
        experienceText = GetComponent<TextMeshProUGUI>();
        experienceText.text = "+"+Experience.ToString() + " XP";
    }
    void Update()
    {
        if(targetPosition!=null )
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition,targetPosition,speed * Time.deltaTime);
            transform.localScale -= new Vector3(0.005f,0.005f,0);

            if(Vector3.Distance(transform.localPosition,targetPosition)<0.3f)
            {
                Destroy(gameObject);
            }
        }
    }
}
