using UnityEngine;

[CreateAssetMenu(fileName = "New Map", menuName = "Data/Map")]
public class MapSO : ScriptableObject
{
    public string Name;
    public Texture2D MapImage;

    [Min(-1)]
    public int MapSceneIndex;
}
