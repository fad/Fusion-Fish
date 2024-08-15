using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class SettingsSO : ScriptableObject
{
    public bool xInputIsInverted;
    public bool yInputIsInverted;
}
