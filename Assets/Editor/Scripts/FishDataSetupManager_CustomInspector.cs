#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(FishDataSetupManager))]
public class FishDataSetupManager_CustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        FishDataSetupManager fishDataSetupManager = (FishDataSetupManager) target;
        
        EditorGUILayout.Space(10f);
        
        if (GUILayout.Button("Setup Fish Data"))
        {
            fishDataSetupManager.SetupFishData();
        }
    }
}

#endif
