using System;
using UnityEngine;

public class AreaMarker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ChangeAreaCheck(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        ChangeAreaCheck(other, false);
    }

    private void ChangeAreaCheck(Collider other, bool isInside)
    {
        bool hasComponent = other.TryGetComponent(out ITreeRunner treeRunner);

        if (hasComponent)
        {
            treeRunner.AdjustAreaCheck(isInside);
        }
    }

    private void OnDrawGizmos()
    {
        TryGetComponent(out BoxCollider boxCollider);

        if (!boxCollider) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, boxCollider.size);
    }
}