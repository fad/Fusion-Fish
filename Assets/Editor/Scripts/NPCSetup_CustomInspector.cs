#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

[CustomEditor(typeof(NPCSetup))]
public class NPCSetup_CustomInspector : Editor
{
    private static readonly Type[] ComponentsToAdd = new []
    {
        typeof(HealthManager),
        typeof(SpawnGibsOnDestroy),
        typeof(NPCHealth),
        typeof(HealthViewModel),
        typeof(AttackManager),
        typeof(StaminaManager)
    };
    
    private const string PathToMeatObjectPrefab = "Assets/Prefabs/MeatObject/MeatObject.prefab";
    private const string PathToEntityDetector = "Assets/Prefabs/System Objects/EntityDetector.prefab";
    private const string PathToHealthbarCanvas = "Assets/Prefabs/System Objects/HealthbarCanvas.prefab";
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        NPCSetup npcSetup = (NPCSetup) target;
        
        EditorGUILayout.Space(10f);
        
        if (GUILayout.Button("Setup"))
        {
            Setup(npcSetup);
        }
    }

    private void Setup(NPCSetup npcSetup)
    {
        if (!npcSetup.CheckFishData())
        {
            Debug.LogError("<color=#f0932b>[NPCSetup]</color>: Cannot setup fish data, no fish data found! Please set it up first!");
            return;
        }
            
        AddObject(npcSetup, out GameObject prefab);
            
        if(!prefab)
        {
            Debug.LogError("<color=#f0932b>[NPCSetup]</color>: Could not create prefab. Model already exists or something in Unity went wrong!");
            return;
        }
            
        AddScripts(prefab);
        InitializeSpawnGibsOnDestroy(prefab);
        CreateEntityDetector(prefab);
        CreateHealthbarCanvas(prefab);
    }

    private void AddObject(NPCSetup toUse, out GameObject prefab)
    {
        if (toUse.transform.Find("Model"))
        {
            prefab = null;
            return;
        }
        
        GameObject modelParent = new GameObject("Model");
        modelParent.transform.SetParent(toUse.transform);
        
        prefab = PrefabUtility.InstantiatePrefab(toUse.Model) as GameObject;
        prefab?.transform.SetParent(modelParent.transform);
    }

    private void AddScripts(GameObject prefab)
    {
        foreach (Type scriptType in ComponentsToAdd)
        {
            if (prefab.GetComponent(scriptType)) continue;
            
            prefab.AddComponent(scriptType);
        }
    }

    private void InitializeSpawnGibsOnDestroy(GameObject prefab)
    {
        SpawnGibsOnDestroy spawnGibsOnDestroy = prefab.GetComponent<SpawnGibsOnDestroy>();
        
        if(!spawnGibsOnDestroy) return;
        
        SerializedObject spawnGibsObject = new SerializedObject(spawnGibsOnDestroy);
        SerializedProperty gibPrefab = spawnGibsObject.FindProperty("gibPrefab");
        
        gibPrefab.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PathToMeatObjectPrefab);
        spawnGibsObject.ApplyModifiedProperties();
    }
    
    private void CreateEntityDetector(GameObject prefab)
    {
        Object detectorPrefab = AssetDatabase.LoadAssetAtPath<Object>(PathToEntityDetector);
        
        if (!detectorPrefab)
        {
            Debug.LogError("Detector prefab not found at path: " + PathToEntityDetector);
            return;
        }
        
        GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab(detectorPrefab);
        instance.name = "Entity Detector";
        instance.transform.SetParent(prefab.transform.parent);
        
        NPCEntityDetector npcEntityDetector = instance.GetComponent<NPCEntityDetector>();
    }
    
    private void CreateHealthbarCanvas(GameObject prefab)
    {
        Object canvasPrefab = AssetDatabase.LoadAssetAtPath<Object>(PathToHealthbarCanvas);
        
        if (!canvasPrefab)
        {
            Debug.LogError("HealthbarCanvas prefab not found at path: " + PathToHealthbarCanvas);
            return;
        }
        
        GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab(canvasPrefab);
        instance.name = "HealthbarCanvas";
        instance.transform.SetParent(prefab.transform.parent);
    }
    
}

#endif
