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
        yield return new WaitUntil(() => UI.Instance != null);

        foreach (var player in Runner.ActivePlayers)
        {
            if (Runner.GetPlayerObject(player).gameObject.GetComponent<NetworkObject>().HasStateAuthority)
            {
                hostPlayerRunner = Runner.GetPlayerObject(player).Runner;
            }
        }

        UI.Instance.playerManager = this;
    }
}
