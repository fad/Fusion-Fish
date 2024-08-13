using System;
using StarterAssets;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public ThirdPersonController thirdPersonController;
    [HideInInspector] public Health health;
    [HideInInspector] public Experience experience;

    private void Start()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        health = GetComponent<Health>();
        experience = GetComponent<Experience>();
    }
}
