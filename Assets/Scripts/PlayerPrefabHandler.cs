using UnityEngine;

public class PlayerPrefabHandler : MonoBehaviour
{
    [SerializeField]
    private AllPlayerPrefabsSO allPlayerPrefabsSO;

    private const string UIFishDataResourcePath = "UI Fish Data";

    private void OnEnable()
    {
        UI_PlayableFishSO[] playerPrefabs = Resources.LoadAll<UI_PlayableFishSO>(UIFishDataResourcePath);
        
        allPlayerPrefabsSO.FillPlayerPrefabs(playerPrefabs);
    }
}
