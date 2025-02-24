using AvocadoShark;
using UnityEngine;

public class MultiplayerPresenter : MonoBehaviour
{
    [SerializeField]
    private FusionConnection fusionConnection;
    
    private const string NamePlayerPrefs = "Name";
    private const string DesiredRoomName = "DesiredRoomName";
    
    private void OnEnable()
    {
        MultiplayerMenuEvents.OnStartHostClicked += OnStartHost;
        MultiplayerMenuEvents.OnFishSelected += OnFishSelected;
        MultiplayerMenuEvents.OnMapSelected += OnMapSelected;
        MultiplayerMenuEvents.OnRegionSelected += OnRegionSelected;
        MultiplayerMenuEvents.OnPlayerHandleChanged += OnPlayerNameChanged;
        MultiplayerMenuEvents.OnSessionNameChanged += OnSessionNameChanged;
    }

    private void Start()
    {
        string playerName = PlayerPrefs.GetString(NamePlayerPrefs, "Player");
        MultiplayerMenuEvents.OnStart_PlayerHandleLoaded?.Invoke(playerName);
    }

    private void OnDisable()
    {
        MultiplayerMenuEvents.OnStartHostClicked -= OnStartHost;
        MultiplayerMenuEvents.OnFishSelected -= OnFishSelected;
        MultiplayerMenuEvents.OnMapSelected -= OnMapSelected;
        MultiplayerMenuEvents.OnRegionSelected -= OnRegionSelected;
        MultiplayerMenuEvents.OnPlayerHandleChanged -= OnPlayerNameChanged;
        MultiplayerMenuEvents.OnSessionNameChanged -= OnSessionNameChanged;
    }

    private void OnSessionNameChanged(string desiredName)
    {
        PlayerPrefs.SetString(DesiredRoomName, desiredName.Length > 0 ? desiredName : "Room");
    }

    private void OnPlayerNameChanged(string playerName)
    {
        PlayerPrefs.SetString(NamePlayerPrefs, playerName);
    }

    private void OnRegionSelected(string region)
    {
        
    }

    private void OnMapSelected(MapSO map)
    {
        fusionConnection.SetSceneNumber(map.MapSceneIndex);
    }

    private void OnFishSelected(UI_PlayableFishSO selectedFish)
    {
        fusionConnection.SetPlayerPrefab(selectedFish.FishNameToDisplay);
    }

    private void OnStartHost()
    {
        fusionConnection.CreateRoom();
    }
}
