using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MapSelection", menuName = "UI List Objects/MapSelection")]
public class MapSelection : ScriptableObject
{
    public MapSelectionData[] mapSelectionData;
}

[Serializable]
public class MapSelectionData
{
    public string name;
    
}
