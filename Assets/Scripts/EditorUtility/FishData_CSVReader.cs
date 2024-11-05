#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class FishData_CSVReader
{
    private static string _pathToCsv = "/Editor/CSVs/FishDataCSV.csv";

    private static Dictionary<string, FishData> _fishDataSOs = new();

    private static string FullPath => Application.dataPath + _pathToCsv;

    private static char _delimiter = ',';
    private static char _listDelimiter = ';';

    [MenuItem("Utilities/Generate Fish Data")]
    public static void GenerateFishData()
    {
        if (!File.Exists(FullPath))
        {
            Debug.LogError("[CSV Reader] <color=#8c7ae6>FishDataCSV.csv</color> not found at path: " + FullPath);
            return;
        }
        
            
        string[] allLines = File.ReadAllLines(FullPath);
        List<SerializedObject> serializedObjects = new List<SerializedObject>();

        for (int i = 1; i < allLines.Length; i++)
        {
            string[] splitData = allLines[i].Split(_delimiter);

            CreateScriptableObject(splitData, out SerializedObject serializedObject);
            serializedObjects.Add(serializedObject);
        }

        for (int j = 1; j < allLines.Length; j++)
        {
            string[] splitData = allLines[j].Split(_delimiter);
            FillHuntValues(serializedObjects[j - 1], splitData[13].Split(_listDelimiter),
                splitData[14].Split(_listDelimiter));
        }

        AssetDatabase.SaveAssets();
    }

    private static void CreateScriptableObject(string[] data, out SerializedObject serializedObject)
    {
        string assetPath = "Assets/ScriptableObjects/FishData/" + data[1] + ".asset";
        
        FishData fishData = AssetDatabase.LoadAssetAtPath<FishData>(assetPath);
        
        if (!fishData)
        {
            fishData = ScriptableObject.CreateInstance<FishData>();
            AssetDatabase.CreateAsset(fishData, assetPath);
        }

        serializedObject = new SerializedObject(fishData);

        FillMetaAndPrefabData(serializedObject, data[0], data[1], data[2], data[3], data[4], data[5]);
        FillGeneralValues(serializedObject, data[6], data[7], data[8], data[9], data[10], data[11], data[12]);
        FillAttackValues(serializedObject, data[15], data[16], data[17], data[18], data[19]);
        FillMovementValues(serializedObject, data[20], data[21], data[22], data[23], data[24]);
        FillFleeValues(serializedObject, data[25]);

        _fishDataSOs.Add(data[1], fishData);
    }

    private static void FillMetaAndPrefabData(SerializedObject dataObject, string ID, string name, string prefabPath,
        string attackComponentName, string staminaComponentName, string scaleValue)
    {
        SerializedProperty fishID = dataObject.FindProperty("fishID");
        SerializedProperty fishPrefab = dataObject.FindProperty("fishPrefab");
        SerializedProperty attackComponent = dataObject.FindProperty("attackComponentName");
        SerializedProperty staminaComponent = dataObject.FindProperty("staminaComponentName");
        SerializedProperty scale = dataObject.FindProperty("scale");

        fishID.intValue = short.Parse(ID);
        fishPrefab.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        attackComponent.stringValue = attackComponentName;
        staminaComponent.stringValue = staminaComponentName;
        scale.floatValue = float.Parse(scaleValue);

        dataObject.ApplyModifiedProperties();
    }

    private static void FillGeneralValues(SerializedObject dataObject, string maxHealthValue, string maxStaminaValue,
        string staminaDecreaseRateValue,
        string staminaRegenRateValue, string staminaThresholdValue, string FOV_AngleValue, string FOV_RadiusValue)
    {
        SerializedProperty maxHealth = dataObject.FindProperty("maxHealth");
        SerializedProperty maxStamina = dataObject.FindProperty("maxStamina");
        SerializedProperty staminaDecreaseRate = dataObject.FindProperty("staminaDecreaseRate");
        SerializedProperty staminaRegenRate = dataObject.FindProperty("staminaRegenRate");
        SerializedProperty staminaThreshold = dataObject.FindProperty("staminaThreshold");
        SerializedProperty FOV_Angle = dataObject.FindProperty("FOV_Angle");
        SerializedProperty FOV_Radius = dataObject.FindProperty("FOV_Radius");

        maxHealth.floatValue = float.Parse(maxHealthValue);
        maxStamina.intValue = short.Parse(maxStaminaValue);
        staminaDecreaseRate.intValue = short.Parse(staminaDecreaseRateValue);
        staminaRegenRate.intValue = short.Parse(staminaRegenRateValue);
        staminaThreshold.intValue = short.Parse(staminaThresholdValue);
        FOV_Angle.floatValue = float.Parse(FOV_AngleValue);
        FOV_Radius.floatValue = float.Parse(FOV_RadiusValue);

        dataObject.ApplyModifiedProperties();
    }

    private static void FillAttackValues(SerializedObject dataObject, string attackDamage, string attackRangeValue,
        string attackCooldownValue,
        string timeToLoseInterestValue, string distanceToLoseInterestValue)
    {
        SerializedProperty attackValue = dataObject.FindProperty("attackValue");
        SerializedProperty attackRange = dataObject.FindProperty("attackRange");
        SerializedProperty attackCooldown = dataObject.FindProperty("attackCooldown");
        SerializedProperty timeToLoseInterest = dataObject.FindProperty("timeToLoseInterest");
        SerializedProperty distanceToLoseInterest = dataObject.FindProperty("distanceToLoseInterest");

        attackValue.floatValue = float.Parse(attackDamage);
        attackRange.floatValue = float.Parse(attackRangeValue);
        attackCooldown.floatValue = float.Parse(attackCooldownValue);
        timeToLoseInterest.floatValue = float.Parse(timeToLoseInterestValue);
        distanceToLoseInterest.floatValue = float.Parse(distanceToLoseInterestValue);

        dataObject.ApplyModifiedProperties();
    }

    private static void
        FillHuntValues(SerializedObject currentDataObject, string[] preyList,
            string[] predatorList) // data[13], data[14]
    {
        SerializedProperty preyListProperty = currentDataObject.FindProperty("preyList");
        SerializedProperty predatorListProperty = currentDataObject.FindProperty("predatorList");

        preyListProperty.ClearArray();
        predatorListProperty.ClearArray();

        List<FishData> preyDataList = new List<FishData>();
        List<FishData> predatorDataList = new List<FishData>();

        foreach (string preyName in preyList)
        {
            if (preyName == "") continue;

            FishData preyData = _fishDataSOs[preyName.TrimStart()];
            preyDataList.Add(preyData);
        }

        foreach (string predatorName in predatorList)
        {
            if (predatorName == "") continue;

            FishData predatorData = _fishDataSOs[predatorName.TrimStart()];
            predatorDataList.Add(predatorData);
        }

        preyListProperty.arraySize = preyDataList.Count;
        for (int i = 0; i < preyDataList.Count; i++)
        {
            preyListProperty.GetArrayElementAtIndex(i).objectReferenceValue = preyDataList[i];
        }

        predatorListProperty.arraySize = predatorDataList.Count;
        for (int i = 0; i < predatorDataList.Count; i++)
        {
            predatorListProperty.GetArrayElementAtIndex(i).objectReferenceValue = predatorDataList[i];
        }

        currentDataObject.ApplyModifiedProperties();
    }

    private static void FillMovementValues(SerializedObject dataObject, string wanderSpeedValue, string fastSpeedValue,
        string rotationSpeedValue,
        string maxPitchValue, string obstacleAvoidanceValue)
    {
        SerializedProperty wanderSpeed = dataObject.FindProperty("wanderSpeed");
        SerializedProperty fastSpeed = dataObject.FindProperty("fastSpeed");
        SerializedProperty rotationSpeed = dataObject.FindProperty("rotationSpeed");
        SerializedProperty maxPitch = dataObject.FindProperty("maxPitch");
        SerializedProperty obstacleAvoidanceDistance = dataObject.FindProperty("obstacleAvoidanceDistance");

        wanderSpeed.floatValue = float.Parse(wanderSpeedValue);
        fastSpeed.floatValue = float.Parse(fastSpeedValue);
        rotationSpeed.floatValue = float.Parse(rotationSpeedValue);
        maxPitch.floatValue = float.Parse(maxPitchValue);
        obstacleAvoidanceDistance.floatValue = float.Parse(obstacleAvoidanceValue);

        dataObject.ApplyModifiedProperties();
    }

    private static void FillFleeValues(SerializedObject dataObject, string safeDistanceValue)
    {
        SerializedProperty safeDistance = dataObject.FindProperty("safeDistance");
        safeDistance.floatValue = float.Parse(safeDistanceValue);

        dataObject.ApplyModifiedProperties();
    }
}
#endif
