#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[CustomEditor(typeof(NPCSetup))]
public class NPCSetup_CustomInspector : Editor
{
    private static readonly Type[] ComponentsToAdd =
    {
        typeof(HealthViewModel),
        typeof(SpawnGibsOnDestroy),
        typeof(AttackManager),
        typeof(StaminaManager)
    };

    private const string PathToMeatObjectPrefab = "Assets/Prefabs/MeatObject/MeatObject.prefab";
    private const string PathToEntityDetector = "Assets/Prefabs/System Objects/EntityDetector.prefab";
    private const string PathToHealthBarCanvas = "Assets/Prefabs/System Objects/HealthbarCanvas.prefab";

    private const string ModelParentName = "Model";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        NPCSetup npcSetup = (NPCSetup)target;

        EditorGUILayout.Space(10f);

        if (GUILayout.Button("Setup"))
        {
            Setup(npcSetup);
        }

        EditorGUILayout.Space(4f);

        if (GUILayout.Button("Update all scripts"))
        {
            UpdateAllScripts(npcSetup);
        }
    }

    private void Setup(NPCSetup npcSetup)
    {
        if (!npcSetup.CheckFishData())
        {
            Debug.LogError(
                "<color=#f0932b>[NPCSetup]</color>: Cannot setup fish data, no fish data found! Please set it up first!");
            return;
        }

        AddObject(npcSetup, out GameObject prefab);

        if (!prefab)
        {
            Debug.LogError(
                "<color=#f0932b>[NPCSetup]</color>: Could not create prefab. Model already exists or something in Unity went wrong!");
            return;
        }

        AddScripts(prefab);
        InitializeSpawnGibsOnDestroy(prefab, npcSetup.FishData);
        InitializeHealthManager(prefab, npcSetup.FishData);
        CreateEntityDetector(prefab, npcSetup.FishData, out NPCEntityDetector npcEntityDetector);
        CreateHealthBarCanvas(prefab, out GameObject healthBarCanvas);
        InitializeHealthViewModel(prefab, healthBarCanvas);
        InitializeDetectionRadius(npcEntityDetector, npcSetup.DetectionRadius);
        InitializeAttackManager(prefab, npcSetup.FishData);
        InitializeStaminaManager(prefab, npcSetup.FishData);
    }

    private void UpdateAllScripts(NPCSetup npcSetup)
    {
        GameObject prefab = npcSetup.transform.Find(ModelParentName)?.gameObject;

        if (!prefab)
        {
            Debug.LogError("<color=#f0932b>[NPCSetup]</color>: Could not find prefab. Please setup first!");
            return;
        }

        InitializeSpawnGibsOnDestroy(prefab, npcSetup.FishData);
        InitializeHealthManager(prefab, npcSetup.FishData);
        InitializeAttackManager(prefab, npcSetup.FishData);
        InitializeStaminaManager(prefab, npcSetup.FishData);
        InitializeDetectionRadius(prefab.transform.parent.GetComponentInChildren<NPCEntityDetector>(),
            npcSetup.DetectionRadius);
    }

    private void AddObject(NPCSetup toUse, out GameObject prefab)
    {
        if (toUse.transform.Find(ModelParentName))
        {
            prefab = null;
            return;
        }
        
        toUse.gameObject.name = toUse.FishData.name;
        toUse.transform.position = Vector3.zero;
        
        prefab = new GameObject(ModelParentName);
        prefab?.transform.SetParent(toUse.transform);
        if (prefab) prefab.transform.position = Vector3.zero;

        GameObject model = PrefabUtility.InstantiatePrefab(toUse.Model) as GameObject;
        model?.transform.SetParent(prefab?.transform);
        if (model) model.transform.position = Vector3.zero;
    }

    private void AddScripts(GameObject prefab)
    {
        foreach (Type scriptType in ComponentsToAdd)
        {
            if (scriptType == typeof(HealthViewModel))
            {
                prefab.transform.parent.gameObject.AddComponent<HealthViewModel>();    
                continue;
            }
            
            if (prefab.GetComponent(scriptType)) continue;

            prefab.AddComponent(scriptType);
        }
    }

    private void InitializeSpawnGibsOnDestroy(GameObject prefab, FishData fishData)
    {
        SpawnGibsOnDestroy spawnGibsOnDestroy = prefab.GetComponent<SpawnGibsOnDestroy>();

        if (!spawnGibsOnDestroy) return;

        SerializedObject spawnGibsObject = new SerializedObject(spawnGibsOnDestroy);
        SerializedProperty gibPrefab = spawnGibsObject.FindProperty("gibPrefab");
    //SerializedProperty gibsXPValue = spawnGibsObject.FindProperty("gibsExperienceValue");

        gibPrefab.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PathToMeatObjectPrefab);
    // gibsXPValue.intValue = fishData.XPValue;

        spawnGibsObject.ApplyModifiedProperties();

        spawnGibsOnDestroy.gibSpawnCount = fishData.GibsSpawnValue;
    }

    private void InitializeHealthManager(GameObject prefab, FishData fishData)
    {
        HealthManager healthManager = prefab.transform.GetComponentInParent<HealthManager>();

        if (!healthManager) return;

        healthManager.maxHealth = fishData.MaxHealth;
        healthManager.recoveryHealthInSecond = fishData.RecoveryHealthInSecond;
        healthManager.timeToStartRecoveryHealth = fishData.TimeToStartRecoveryHealth;
    }

    private void InitializeHealthViewModel(GameObject prefab, GameObject healthBarCanvas)
    {
        HealthViewModel healthViewModel = prefab.GetComponentInParent<HealthViewModel>();

        if (!healthViewModel) return;

        SerializedObject healthViewModelObject = new SerializedObject(healthViewModel);
        SerializedProperty HM = healthViewModelObject.FindProperty("healthModel");
        SerializedProperty HS = healthViewModelObject.FindProperty("healthSlider");

        HM.objectReferenceValue = prefab.transform.GetComponentInParent<HealthManager>();
        HS.objectReferenceValue = healthBarCanvas.GetComponentInChildren<Slider>(true);

        healthViewModelObject.ApplyModifiedProperties();
    }

    private void InitializeDetectionRadius(NPCEntityDetector npcEntityDetector, float radius)
    {
        SphereCollider sphereCollider = npcEntityDetector.GetComponent<SphereCollider>();

        if (!sphereCollider) return;

        sphereCollider.radius = radius;
    }

    private void InitializeAttackManager(GameObject prefab, FishData fishData)
    {
        AttackManager attackManager = prefab.GetComponent<AttackManager>();

        if (!attackManager) return;

        SerializedObject attackManagerObject = new SerializedObject(attackManager);
        SerializedProperty fishDataProperty = attackManagerObject.FindProperty("fishData");

        fishDataProperty.objectReferenceValue = fishData;

        attackManagerObject.ApplyModifiedProperties();
    }

    private void InitializeStaminaManager(GameObject prefab, FishData fishData)
    {
        StaminaManager staminaManager = prefab.GetComponent<StaminaManager>();

        if (!staminaManager) return;

        SerializedObject staminaManagerObject = new SerializedObject(staminaManager);
        SerializedProperty fishDataProperty = staminaManagerObject.FindProperty("fishData");

        fishDataProperty.objectReferenceValue = fishData;

        staminaManagerObject.ApplyModifiedProperties();
    }

    private void CreateEntityDetector(GameObject prefab, FishData fishData, out NPCEntityDetector npcEntityDetector)
    {
        Object detectorPrefab = AssetDatabase.LoadAssetAtPath<Object>(PathToEntityDetector);

        if (!detectorPrefab)
        {
            Debug.LogError("Detector prefab not found at path: " + PathToEntityDetector);
            npcEntityDetector = null;
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(detectorPrefab);
        instance.name = "Entity Detector";
        instance.transform.SetParent(prefab.transform.parent);
        instance.transform.position = Vector3.zero;

        npcEntityDetector = instance.GetComponent<NPCEntityDetector>();

        SerializedObject detectorObject = new SerializedObject(npcEntityDetector);
        SerializedProperty root = detectorObject.FindProperty("root");

        root.objectReferenceValue = prefab.transform.parent;

        detectorObject.ApplyModifiedProperties();
        npcEntityDetector.Init(fishData.name);
    }

    private void CreateHealthBarCanvas(GameObject prefab, out GameObject healthBarCanvas)
    {
        Object canvasPrefab = AssetDatabase.LoadAssetAtPath<Object>(PathToHealthBarCanvas);

        if (!canvasPrefab)
        {
            Debug.LogError("HealthBarCanvas prefab not found at path: " + PathToHealthBarCanvas);
            healthBarCanvas = null;
            return;
        }

        healthBarCanvas = (GameObject)PrefabUtility.InstantiatePrefab(canvasPrefab);
        healthBarCanvas.name = "HealthBarCanvas";
        healthBarCanvas.transform.SetParent(prefab.transform.parent);
        healthBarCanvas.transform.position = Vector3.zero;
        
        healthBarCanvas.transform.GetChild(0).gameObject.SetActive(false);
    }
}

#endif
