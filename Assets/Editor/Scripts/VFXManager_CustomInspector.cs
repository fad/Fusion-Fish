using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VFXManager))]
public class VFXManager_CustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        VFXManager vfxManager = (VFXManager)target;

        EditorGUILayout.Space(10f);
        
        if (GUILayout.Button("Show Hurt Vignette"))
        {
            vfxManager.ShowHurtVignette();
        }
    }
}
