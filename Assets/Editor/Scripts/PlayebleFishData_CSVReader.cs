#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Globalization;

public class PlayableFishData_CSVReader
{
    private static string _pathToCsv = "/Editor/CSVs/PlayableFishData.csv";
    private static string _pathToSave = "Assets/Resources/FishData/PlayableFishData/";

    private static Dictionary<string, PlayerFishData> _fishDataSOs = new();
    private static Dictionary<string, PlayerFishData> _playableFishDataSOs = new();

    private static string FullPath => Application.dataPath + _pathToCsv;

    private static char _delimiter = ',';
    private static char _listDelimiter = ';';

    [MenuItem("Utilities/Generate Playable Fish Data")]
    public static void GenerateFishData()
    {
        if (!File.Exists(FullPath))
        {
            Debug.LogError("[CSV Reader] <color=#8c7ae6>PlayableFishData.csv</color> not found at path: " + FullPath);
            return;
        }

        _fishDataSOs.Clear();
        _playableFishDataSOs.Clear();

        string[] allLines = File.ReadAllLines(FullPath);
        List<SerializedObject> serializedObjects = new List<SerializedObject>();

        for (int i = 1; i < allLines.Length; i++)
        {
            string[] splitData = allLines[i].Split(_delimiter);

            CreateScriptableObject(splitData, out SerializedObject serializedObject);
            serializedObjects.Add(serializedObject);
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateScriptableObject(string[] data, out SerializedObject serializedObject)
    {
        string assetPath = _pathToSave + data[1] + ".asset";

        PlayerFishData fishData = AssetDatabase.LoadAssetAtPath<PlayerFishData>(assetPath);

        if (!fishData)
        {
            fishData = ScriptableObject.CreateInstance<PlayerFishData>();
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
            xpValue: data[10],
            gibsSpawnValue: data[11]);

        FillAttackValues(serializedObject,
            attackDamage: data[12],
            attackRangeValue: data[13]);

        FillMovementValues(serializedObject,
            wanderSpeedValue: data[14],
            fastSpeedValue: data[15],
            rotationSpeedValue: data[16],
            maxPitchValue: data[17]);

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
        scale.floatValue = float.Parse(scaleValue, CultureInfo.InvariantCulture);

        dataObject.ApplyModifiedProperties();
    }

    private static void FillGeneralValues(SerializedObject dataObject, string maxHealthValue,
        string recoveryHealthInSec, string timeToStartRecovering, string maxStaminaValue,
        string staminaDecreaseRateValue,string staminaRegenRateValue, string xpValue, string gibsSpawnValue)
    {
        SerializedProperty maxHealth = dataObject.FindProperty("maxHealth");
        SerializedProperty recoveryHealthInSecProperty = dataObject.FindProperty("recoveryHealthInSecond");
        SerializedProperty timeToStartRecoveringProperty = dataObject.FindProperty("timeToStartRecoveryHealth");
        SerializedProperty maxStamina = dataObject.FindProperty("maxStamina");
        SerializedProperty staminaDecreaseRate = dataObject.FindProperty("staminaDecreaseRate");
        SerializedProperty staminaRegenRate = dataObject.FindProperty("staminaRegenRate");
        SerializedProperty xpValueProperty = dataObject.FindProperty("xpValue");
        SerializedProperty gibsSpawnValueProperty = dataObject.FindProperty("gibsSpawnValue");

        maxHealth.floatValue = float.Parse(maxHealthValue, CultureInfo.InvariantCulture);
        recoveryHealthInSecProperty.floatValue = float.Parse(recoveryHealthInSec, CultureInfo.InvariantCulture);
        timeToStartRecoveringProperty.floatValue = float.Parse(timeToStartRecovering, CultureInfo.InvariantCulture);
        maxStamina.intValue = short.Parse(maxStaminaValue, CultureInfo.InvariantCulture);
        staminaDecreaseRate.intValue = short.Parse(staminaDecreaseRateValue, CultureInfo.InvariantCulture);
        staminaRegenRate.intValue = short.Parse(staminaRegenRateValue, CultureInfo.InvariantCulture);
        xpValueProperty.intValue = short.Parse(xpValue, CultureInfo.InvariantCulture);
        gibsSpawnValueProperty.intValue = short.Parse(gibsSpawnValue, CultureInfo.InvariantCulture);

        dataObject.ApplyModifiedProperties();
    }

    private static void FillAttackValues(SerializedObject dataObject, string attackDamage, string attackRangeValue)
    {
        SerializedProperty attackValue = dataObject.FindProperty("attackValue");
        SerializedProperty attackRange = dataObject.FindProperty("attackRange");

        attackValue.floatValue = float.Parse(attackDamage, CultureInfo.InvariantCulture);
        attackRange.floatValue = float.Parse(attackRangeValue, CultureInfo.InvariantCulture);

        dataObject.ApplyModifiedProperties();
    }

    private static void FillMovementValues(SerializedObject dataObject, string wanderSpeedValue, string fastSpeedValue,
        string rotationSpeedValue,string maxPitchValue)
    {
        SerializedProperty wanderSpeed = dataObject.FindProperty("wanderSpeed");
        SerializedProperty fastSpeed = dataObject.FindProperty("fastSpeed");
        SerializedProperty rotationSpeed = dataObject.FindProperty("rotationSpeed");
        SerializedProperty maxPitch = dataObject.FindProperty("maxPitch");

        wanderSpeed.floatValue = float.Parse(wanderSpeedValue, CultureInfo.InvariantCulture);
        fastSpeed.floatValue = float.Parse(fastSpeedValue, CultureInfo.InvariantCulture);
        rotationSpeed.floatValue = float.Parse(rotationSpeedValue, CultureInfo.InvariantCulture);
        maxPitch.floatValue = float.Parse(maxPitchValue, CultureInfo.InvariantCulture);

        dataObject.ApplyModifiedProperties();
    }
}
#endif
