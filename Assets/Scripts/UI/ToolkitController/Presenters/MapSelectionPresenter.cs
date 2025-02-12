using AvocadoShark;
using UnityEngine;

public class MapSelectionPresenter : MonoBehaviour
{
    [SerializeField]
    private FusionConnection fusionConnection;

    [SerializeField]
    private MapSO standardMap;

    private void OnEnable()
    {
        MapSelectionEvents.OnMapSelected += OnMapSelected;
        
        fusionConnection.SetSceneNumber(standardMap.MapSceneIndex);
    }


    private void OnDisable()
    {
        MapSelectionEvents.OnMapSelected -= OnMapSelected;
    }
    
    
    private void OnMapSelected(MapSO selectedMap)
    {
        fusionConnection.SetSceneNumber(selectedMap.MapSceneIndex);
    }
}
