using UnityEngine;

[CreateAssetMenu(fileName = "New Playable Fish", menuName = "Data/UI: Playable Fish")]
public class UI_PlayableFishSO : ScriptableObject
{
    public string FishNameToDisplay;
    
    [SerializeField]
    private PlayerFishData playerFishData;

    public PlayerFishData PlayableFish => playerFishData;
}
