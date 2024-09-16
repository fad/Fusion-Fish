using System;
using System.Collections;
using Fusion;
using StarterAssets;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [HideInInspector] public ThirdPersonController thirdPersonController;
    [HideInInspector] public Health health;
    [HideInInspector] public Experience experience;
    [SerializeField] public Attack attack;
    [HideInInspector] public NetworkRunner hostPlayerRunner;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        health = GetComponent<Health>();
        experience = GetComponent<Experience>();
    }
    
    private IEnumerator Start()
    {
        foreach (var unused in Runner.ActivePlayers)
        {
            if (GetComponent<NetworkObject>().HasStateAuthority)
            {
                hostPlayerRunner = Runner;
            }
        }
        
        yield return new WaitUntil(() => UI.Instance != null);

        if (HasStateAuthority)
        {
            UI.Instance.playerManager = this;
        }
    }
}
