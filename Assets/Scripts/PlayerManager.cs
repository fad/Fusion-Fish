using System;
using StarterAssets;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public ThirdPersonController thirdPersonController;
    [HideInInspector] public Health health;
    [HideInInspector] public Experience experience;
    [SerializeField] public Attack attack;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        health = GetComponent<Health>();
        experience = GetComponent<Experience>();
    }
}
