using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Player Spawn Locations")]
    [HideInInspector] public Vector3 chosenLocation;
    [HideInInspector] public Quaternion chosenRotation;
    [SerializeField] public Vector3 customLocation1;
    [SerializeField] public Quaternion customRotation1;
    [SerializeField] public Vector3 customLocation2;
    [SerializeField] public Quaternion customRotation2;
    [SerializeField] public Vector3 customLocation3;
    [SerializeField] public Quaternion customRotation3;

    public static SpawnManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
