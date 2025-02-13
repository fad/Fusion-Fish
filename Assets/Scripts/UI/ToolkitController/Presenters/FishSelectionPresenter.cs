using System;
using AvocadoShark;
using UnityEngine;

public class FishSelectionPresenter : MonoBehaviour
{
    [SerializeField]
    private FusionConnection fusionConnection;

    [SerializeField]
    private UI_PlayableFishSO standardFish;

    private void OnEnable()
    {
        FishSelectionEvents.OnFishButtonClicked += OnFishButtonClicked;
        FishSelectionEvents.OnStartButtonClicked += OnStartButtonClicked;
    }

    private void Start()
    {
        fusionConnection.SetPlayerPrefab(standardFish.FishNameToDisplay);
    }


    private void OnDisable()
    {
        FishSelectionEvents.OnFishButtonClicked -= OnFishButtonClicked;
        FishSelectionEvents.OnStartButtonClicked -= OnStartButtonClicked;
    }

    private void OnFishButtonClicked(UI_PlayableFishSO selectedFish)
    {
        fusionConnection.SetPlayerPrefab(selectedFish.FishNameToDisplay);
    }
    
    private void OnStartButtonClicked()
    {
        fusionConnection.CreateRoomSingleplayer();
    }
}
