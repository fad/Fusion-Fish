﻿using Fusion;
using UnityEngine;

public class SuckableService : NetworkBehaviour, ISuckable
{
    [SerializeField]
    private int experienceValue = 100;

    [SerializeField,
     Tooltip("The amount of sucking power needed to suck this object in. Usually the max health of the object.")]
    private float neededSuckingPower = 1f;

    public float NeededSuckingPower => neededSuckingPower;

    public int GetSuckedIn()
    {
        if(Object == null)
            return 0;
        DestroySuckableRpc();
        return experienceValue;
    }

    public void SetupXP(int xp)
    {
        SameXPValueRpc(xp);
    }

    // BUG: This throws a InvalidOperationException: InvalidOperationException: Behaviour not initialized: Object not set.
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void DestroySuckableRpc()
    {
        Runner.Despawn(Object);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void SameXPValueRpc(int xp)
    {
        experienceValue = xp;
    }
}
