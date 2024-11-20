using UnityEngine;

/// <summary>
/// Entity detector for player objects
/// </summary>
public class PlayerEntityDetector : EntityDetector
{
    // protected override void OnTriggerEnter(Collider other)
    // {
    //     if (IsNotValid(other.gameObject)) return;

    //     DealWithHealthUI(other.gameObject, true);
    // }

    // protected override void OnTriggerExit(Collider other)
    // {
    //     if (IsNotValid(other.gameObject)) return;

    //     DealWithHealthUI(other.gameObject, false);
    // }

    // private void DealWithHealthUI(GameObject entity, bool shouldShowHealthUI)
    // {
    //    bool hasHealthViewModel = entity.TryGetComponent(out HealthViewModel healthViewModel);
    //    if (!hasHealthViewModel) return;


    //    healthViewModel.AdjustHealthBarVisibility(shouldShowHealthUI);
    // }
}
