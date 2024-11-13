#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ContextMenuUtils
{
    private const string PathToFishAreaPrefab = "Assets/Prefabs/Levelelements/FishArea.prefab";
    private const string PathToSpawnPointPrefab = "Assets/Prefabs/Levelelements/FishSpawnPoint.prefab";
    private const string PathToHatcheryPrefab = "Assets/Prefabs/Levelelements/SpawningGround.prefab";
    
    [MenuItem("GameObject/Level Elements/Fish Area", false, 4)]
    public static void AddFishArea()
    {
        InstantiatePrefabAtPath(PathToFishAreaPrefab, "Fish Area");
    }
    
    [MenuItem("GameObject/Level Elements/Spawn Point", false, 4)]
    public static void AddSpawnPoint()
    {
        InstantiatePrefabAtPath(PathToSpawnPointPrefab, "Spawn Point");
    }
    
    [MenuItem("GameObject/Level Elements/Hatchery", false, 4)]
    public static void AddHatchery()
    {
        InstantiatePrefabAtPath(PathToHatcheryPrefab, "Hatchery");
    }
    
    private static void InstantiatePrefabAtPath(string path, string name)
    {
        Object prefab = AssetDatabase.LoadAssetAtPath<Object>(path);
        
        if (prefab)
        {
            GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.position = Vector3.zero;

            if (Selection.activeGameObject)
            {
                instance.transform.SetParent(Selection.activeGameObject.transform);
            }
            
            Selection.activeGameObject = instance;
        }
        else
        {
            Debug.LogError(name + " prefab not found at path: " + path);
        }
    }
    
    
}

#endif