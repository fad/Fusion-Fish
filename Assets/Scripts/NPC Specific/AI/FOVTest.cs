using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVTest : MonoBehaviour
{
    public float viewRadius = 45f;
    public float viewDistance = 10f;
    
    public Transform target;

    private void Update()
    {
        target.transform.RotateAround(transform.position, Vector3.up, 20 * Time.deltaTime);
        //DebugOnSight();
    }

    private void OnDrawGizmos()
    {
        Quaternion leftRayRotation = Quaternion.AngleAxis(-viewRadius, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(viewRadius, Vector3.up);
        
        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;
        
        Gizmos.color = new Color(255f/255f, 165f/255f, 0f/255f, 1f);
        Gizmos.DrawLine(transform.position, transform.position + leftRayDirection * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightRayDirection * viewDistance);
        
    }

    private void DebugOnSight()
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        
        if (angleToTarget < viewRadius)
        {
            Debug.Log("On Sight");
            Debug.Log($"Distance: {Vector3.Distance(transform.position, target.position)}");
        }
    }

}
