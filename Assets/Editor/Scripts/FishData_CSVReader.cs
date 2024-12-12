#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class FishData_CSVReader
{
    private static string _pathToCsv = "/Editor/CSVs/FishDataCSV.csv";
    private static string _pathToSave = "Assets/Resources/FishData/";

    private static Dictionary<string, FishData> _fishDataSOs = new();
    private static Dictionary<string, FishData> _playableFishDataSOs = new();

    private static string FullPath => Application.dataPath + _pathToCsv;

    private static char _delimiter = ',';
    private static char _listDelimiter = ';';

    private static string _playablePrefix = "Playable_";

    [MenuItem("Utilities/Generate Fish Data")]
    public static void GenerateFishData()
    {
        if (!File.Exists(FullPath))
        {
            Debug.LogError("[CSV Reader] <color=#8c7ae6>FishDataCSV.csv</color> not found at path: " + FullPath);
            return;
        }

        _fishDataSOs.Clear();
        _playableFishDataSOs.Clear();

        string[] allLines = File.ReadAllLines(FullPath);
        List<SerializedObject> serializedObjects = new List<SerializedObject>();

        for (int i = 1; i < allLines.Length; i++)
        {
            string[] splitData = allLines[i].Split(_delimiter);

            Debug.Log(i);
            CreateScriptableObject(splitData, out SerializedObject serializedObject);
            serializedObjects.Add(serializedObject);
        }

        for (int j = 1; j < allLines.Length; j++)
        {
            string[] splitData = allLines[j].Split(_delimiter);
            FillHuntValues(serializedObjects[j - 1], splitData[15].Split(_listDelimiter),
                splitData[16].Split(_listDelimiter));
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateScriptableObject(string[] data, out SerializedObject serializedObject)
    {
        string assetPath = _pathToSave + data[1] + ".asset";

        FishData fishData = AssetDatabase.LoadAssetAtPath<FishData>(assetPath);

        if (!fishData)
        {
            fishData = ScriptableObject.CreateInstance<FishData>();
            AssetDatabase.CreateAsset(fishData, assetPath);
        }

        serializedObject = new SerializedObject(fishData);

        FillMetaAndPrefabData(serializedObject, data[0], data[1], data[2], data[3]);
        FillGeneralValues(serializedObject,
            maxHealthValue: data[4],
            recoveryHealthInSec: data[6],
            timeToStartRecovering: data[7],
            maxStaminaValue: data[5],
            staminaDecreaseRateValue: data[8],
            staminaRegenRateValue: data[9],
            staminaThresholdValue: data[10],
            xpValue: data[11],
            gibsSpawnValue: data[12],
            FOV_AngleValue: data[13],
            FOV_RadiusValue: data[14]);

        FillAttackValues(serializedObject,
            attackDamage: data[17],
            attackRangeValue: data[18],
            attackCooldownValue: data[19],
            timeToLoseInterestValue: data[20],
            distanceToLoseInterestValue: data[21]);

        FillMovementValues(serializedObject,
            wanderSpeedValue: data[22],
            fastSpeedValue: data[23],
            rotationSpeedValue: data[24],
            maxPitchValue: data[25],
            obstacleAvoidanceValue: data[26],
            wanderDistanceVerticalValue: data[28],
            wanderDistanceHorizontalValue: data[29]);

        FillFleeValues(serializedObject,
            safeDistanceValue: data[27]);

        if (data[1].StartsWith(_playablePrefix))
        {
            _playableFishDataSOs.Add(data[1].Replace(_playablePrefix, ""), fishData);
            return;
        }

        _fishDataSOs.Add(data[1], fishData);
    }

    private static void FillMetaAndPrefabData(SerializedObject dataObject, string ID, string name, string prefabPath,
        string scaleValue)
    {
        SerializedProperty fishID = dataObject.FindProperty("fishID");
        SerializedProperty fishPrefab = dataObject.FindProperty("fishPrefab");
        SerializedProperty scale = dataObject.FindProperty("scale");

        fishID.intValue = short.Parse(ID);
        fishPrefab.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        //scale.floatValue = float.Parse(scaleValue);

        dataObject.ApplyModifiedProperties();
    }

    private static void FillGeneralValues(SerializedObject dataObject, string maxHealthValue,
        string recoveryHealthInSec, string timeToStartRecovering, string maxStaminaValue,
        string staminaDecreaseRateValue,
        string staminaRegenRateValue, string staminaThresholdValue, string xpValue, string gibsSpawnValue,
        string FOV_AngleValue, string FOV_RadiusValue)
    {
        SerializedProperty maxHealth = dataObject.FindProperty("maxHealth");
        SerializedProperty recoveryHealthInSecProperty = dataObject.FindProperty("recoveryHealthInSecond");
        SerializedProperty timeToStartRecoveringProperty = dataObject.FindProperty("timeToStartRecoveryHealth");
        SerializedProperty maxStamina = dataObject.FindProperty("maxStamina");
        SerializedProperty staminaDecreaseRate = dataObject.FindProperty("staminaDecreaseRate");
        SerializedProperty staminaRegenRate = dataObject.FindProperty("staminaRegenRate");
        SerializedProperty staminaThreshold = dataObject.FindProperty("staminaThreshold");
        SerializedProperty xpValueProperty = dataObject.FindProperty("xpValue");
        SerializedProperty gibsSpawnValueProperty = dataObject.FindProperty("gibsSpawnValue");
        SerializedProperty FOV_Angle = dataObject.FindProperty("FOV_Angle");
        SerializedProperty FOV_Radius = dataObject.FindProperty("FOV_Radius");

        maxHealth.floatValue = float.Parse(maxHealthValue);
        recoveryHealthInSecProperty.floatValue = float.Parse(recoveryHealthInSec);
        timeToStartRecoveringProperty.floatValue = float.Parse(timeToStartRecovering);
        maxStamina.intValue = short.Parse(maxStaminaValue);
        staminaDecreaseRate.intValue = short.Parse(staminaDecreaseRateValue);
        staminaRegenRate.intValue = short.Parse(staminaRegenRateValue);
        staminaThreshold.intValue = short.Parse(staminaThresholdValue);
        xpValueProperty.intValue = short.Parse(xpValue);
        gibsSpawnValueProperty.intValue = short.Parse(gibsSpawnValue);
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
            string[] predatorList)
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

            FishData preyData = _playableFishDataSOs.ContainsKey(preyName.TrimStart())
                ? _playableFishDataSOs[preyName.TrimStart()]
                : _fishDataSOs[preyName.TrimStart()];

            preyDataList.Add(preyData);
        }

        foreach (string predatorName in predatorList)
        {
            if (predatorName == "") continue;

            FishData predatorData = _playableFishDataSOs.ContainsKey(predatorName.TrimStart())
                ? _playableFishDataSOs[predatorName.TrimStart()]
                : _fishDataSOs[predatorName.TrimStart()];
            
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
        string maxPitchValue, string obstacleAvoidanceValue, string wanderDistanceVerticalValue, string wanderDistanceHorizontalValue)
    {
        SerializedProperty wanderSpeed = dataObject.FindProperty("wanderSpeed");
        SerializedProperty fastSpeed = dataObject.FindProperty("fastSpeed");
        SerializedProperty rotationSpeed = dataObject.FindProperty("rotationSpeed");
        SerializedProperty maxPitch = dataObject.FindProperty("maxPitch");
        SerializedProperty obstacleAvoidanceDistance = dataObject.FindProperty("obstacleAvoidanceDistance");
        SerializedProperty wanderDistanceVertical = dataObject.FindProperty("wanderDistanceVertical");
        SerializedProperty wanderDistanceHorizontal = dataObject.FindProperty("wanderDistanceHorizontal");

        wanderSpeed.floatValue = float.Parse(wanderSpeedValue);
        fastSpeed.floatValue = float.Parse(fastSpeedValue);
        rotationSpeed.floatValue = float.Parse(rotationSpeedValue);
        maxPitch.floatValue = float.Parse(maxPitchValue);
        obstacleAvoidanceDistance.floatValue = float.Parse(obstacleAvoidanceValue);
        wanderDistanceVertical.floatValue = float.Parse(wanderDistanceVerticalValue);
        wanderDistanceHorizontal.floatValue = float.Parse(wanderDistanceHorizontalValue);

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
