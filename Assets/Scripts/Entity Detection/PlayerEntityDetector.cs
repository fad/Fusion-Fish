using AvocadoShark;
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
        DealWithPlayerName(other.gameObject, true);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (!TryCheck(other.gameObject)) return;

        DealWithFoodOutline(other.gameObject, false);
        DealWithPlayerName(other.gameObject, false);
    }

    private void DealWithFoodOutline(GameObject other, bool shouldOutline)
    {
        if (other.TryGetComponent(out OutlineManager outlineManager) && other.TryGetComponent(out SuckableService _))
        {
            outlineManager.ShouldOutline(shouldOutline);
        }
    }

    private void DealWithPlayerName(GameObject other, bool shouldShow)
    {
        if (other.TryGetComponent(out PlayerStats playerStats))
        {
            playerStats.SetPlayerNameVisibility(shouldShow);
        }
    }
}
