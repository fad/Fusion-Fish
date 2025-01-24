using System.Collections.Generic;
using AvocadoShark;
using UnityEngine;

/// <summary>
/// Entity detector for player objects
/// </summary>
public class PlayerEntityDetector : EntityDetector
{
    private readonly HashSet<OutlineManager> _foodOutlines = new();
    
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
        
        if (shouldOutline)
        {
            _foodOutlines.Add(outlineManager);
        }
        else
        {
            _foodOutlines.Remove(outlineManager);
        }
    }

    private void DealWithPlayerName(GameObject other, bool shouldShow)
    {
        if (other.TryGetComponent(out PlayerStats playerStats))
        {
            playerStats.SetPlayerNameVisibility(shouldShow);
        }
    }
    
    public void DisableAllOutlines()
    {
        foreach (var outline in _foodOutlines)
        {
            outline.ShouldOutline(false);
            _foodOutlines.Remove(outline);
        }
    }
}
