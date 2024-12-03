using UnityEngine;

/// <summary>
/// Entity detector for player objects
/// </summary>
public class PlayerEntityDetector : EntityDetector
{
    protected override void OnTriggerEnter(Collider other)
    {
        if (!TryCheck(other.gameObject)) return;

        DealWithFoodOutline(other.gameObject, true);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (!TryCheck(other.gameObject)) return;

        DealWithFoodOutline(other.gameObject, false);
    }

    private void DealWithFoodOutline(GameObject other, bool shouldOutline)
    {
        if (other.TryGetComponent(out OutlineManager outlineManager) && other.TryGetComponent(out SuckableService _))
        {
            outlineManager.ShouldOutline(shouldOutline);
        }
    }
}
