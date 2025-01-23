using Fusion;
using UnityEngine;

public class StateAuthManager : NetworkBehaviour, IStateAuthorityChanged
{
    public void StateAuthorityChanged()
    {
        if (Object.StateAuthority == PlayerRef.None)
        {
            Debug.Log("Authority changed to None");
        }
    }
}
