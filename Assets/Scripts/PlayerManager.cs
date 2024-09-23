using System;
using System.Collections;
using Fusion;
using StarterAssets;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : NetworkBehaviour
{
    [HideInInspector] public ThirdPersonController thirdPersonController;
    [HideInInspector] public Health health;
    [FormerlySerializedAs("experience")] [HideInInspector] public LevelUp levelUp;
    [SerializeField] public Attack attack;
    [HideInInspector] public NetworkRunner hostPlayerRunner;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        health = GetComponent<Health>();
        levelUp = GetComponent<LevelUp>();
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
        
        yield return new WaitUntil(() => HudUI.Instance != null);

        if (HasStateAuthority)
        {
            HudUI.Instance.playerManager = this;
        }
    }
}
