using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllPlayerPrefabsSO", menuName = "Data/All Player Prefabs")]
public class AllPlayerPrefabsSO : ScriptableObject
{
    private Dictionary<string, UI_PlayableFishSO> PlayerPrefabs { get; } = new();
    
    public UI_PlayableFishSO this[string key] => PlayerPrefabs[key];
    
    public void FillPlayerPrefabs(UI_PlayableFishSO[] playerPrefabs)
    {
        foreach (UI_PlayableFishSO playerPrefab in playerPrefabs)
        {
            PlayerPrefabs.Add(playerPrefab.FishNameToDisplay, playerPrefab);
        }
    }
    
}
