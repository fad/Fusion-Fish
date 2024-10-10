using System.Linq;
using UnityEngine;
using Fusion;

public class AuthorityHandler : NetworkBehaviour, IStateAuthorityChanged
{
	/// <summary>
	/// Unique identifier for the network object. Needed for distinctly recognizing the object in the network.
	/// </summary>
	[Networked] System.Guid Guid { get; set; }

	/// <summary>
	/// Flag to track whether the authority of the networked object is currently in the process of changing.
	/// Prevents multiple authority changes from occurring simultaneously.
	/// Ensures that the state transition is handled correctly and avoids potential conflicts.
	/// </summary>
	private bool isAuthorityChanging = false;
	
	/// <summary>
	/// Simulation time when an authority request was made.
	/// Used for handling timeouts for pending authority requests.
	/// </summary>
	float timeRequested = 0;
	
	/// <summary>
	/// Action to invoke when authority is granted.
	/// </summary>
	System.Action onAuthorized = null;
	
	/// <summary>
	/// Action to invoke when authority is not granted.
	/// </summary>
	System.Action onUnauthorized = null;

	/*
	 * Ensures that the GUID is initialized when the object is spawned.
	 * Will only generate a GUID once.
	 */
	public override void Spawned()
	{
		if (Object.HasStateAuthority) Guid = System.Guid.NewGuid();
	}

	/// <summary>
	/// Request authority for this object.
	/// Will request authority if the object has no authority.
	/// Otherwise will transfer authority to the local player.
	/// </summary>
	/// <param name="onAuthorized">Action to use when authority was granted</param>
	/// <param name="onUnauthorized">Action to use when authority was not granted</param>
	public void RequestAuthority(System.Action onAuthorized = null, System.Action onUnauthorized = null)
	{
		timeRequested = Runner.SimulationTime;
		this.onAuthorized = onAuthorized;
		this.onUnauthorized = onUnauthorized;

		bool hasStateAuthority = !Object.StateAuthority.IsNone;
		bool stateAuthorityIsActivePlayer = Runner.ActivePlayers.FirstOrDefault(p => p == Object.StateAuthority) != default;

		if (hasStateAuthority && stateAuthorityIsActivePlayer)
		{
			Rpc_RequestAuthority(Object.StateAuthority, GetHierarchyState());
		}
		else
		{
			Rpc_Authorized(Runner.LocalPlayer);
		}
	}

	/// <summary>
	/// Calculates the GUID that represents the combined state of all AuthorityHandler components in the hierarchy.
	/// Ensures that the state of the entire hierarchy is considered.
	/// Helps in validating that the state of the hierarchy did not change so that network synchronization can be maintained.
	/// </summary>
	/// <returns>Combined GUID of the hierarchy</returns>
	private System.Guid GetHierarchyState()
	{
		byte[] g1 = new byte[16]; // Initialized to all zeros
		
		foreach (var authHandler in GetComponentsInChildren<AuthorityHandler>()) // Iterating so the entire hierarchy is considered
		{
			byte[] g2 = authHandler.Guid.ToByteArray(); // Get the GUID as a byte array
			
			for (var i = 0; i < 16; i++) 
			{
				g1[i] ^= g2[i]; // XOR for combining the GUIDs so we have a single GUID representing the entire hierarchy
			}
		}
		return new System.Guid(g1);
	}
	
	
	/// <summary>
	/// Handles requests for authority over a networked object.
	/// </summary>
	/// <param name="player">Player requesting authority</param>
	/// <param name="expectedState">The expected state of the hierarchy</param>
	/// <param name="info">Additional RPC information (default value provided)</param>
	[Rpc(RpcSources.All, RpcTargets.All)]
	private void Rpc_RequestAuthority([RpcTarget] PlayerRef player, System.Guid expectedState, RpcInfo info = default)
	{
		if (Object.HasStateAuthority && !isAuthorityChanging && expectedState.Equals(GetHierarchyState()))
		{
			if (info.IsInvokeLocal) // If the RPC was invoked locally
			{
				onAuthorized?.Invoke();
				onAuthorized = null;
				onUnauthorized = null;
				timeRequested = 0;
			}
			else // If the RPC was invoked remotely
			{
				isAuthorityChanging = true;
				Rpc_Authorized(info.Source);
				Log($"authorizing {info.Source} for {gameObject.name}", gameObject);
			}
		}
		else
		{
			Rpc_NotAuthorized(info.Source);
		}
	}

	/// <summary>
	/// Handles the authorization process of a networked object. Authority is granted to the local player.
	/// </summary>
	/// <param name="player">The player to grant the StateAuthority to.</param>
	[Rpc(RpcSources.All, RpcTargets.All)]
	private void Rpc_Authorized([RpcTarget] PlayerRef player)
	{
		Log("authorized...");
		AuthorityHandler[] authHandlers = GetComponentsInChildren<AuthorityHandler>();
		foreach (AuthorityHandler authHandler in authHandlers)
		{
			Log(authHandler.gameObject.name, gameObject);
			authHandler.Object.RequestStateAuthority();
		}
	}

	/// <summary>
	/// Handles the scenario when a request for authority over a networked object is denied.
	/// </summary>
	/// <param name="player">Player whose request was denied.</param>
	[Rpc(RpcSources.All, RpcTargets.All)]
	private void Rpc_NotAuthorized([RpcTarget] PlayerRef player)
	{
		Log("not authorized");
		onUnauthorized?.Invoke();
		onAuthorized = null;
		onUnauthorized = null;
	}

	/// <summary>
	/// Handles the despawning of a networked object.
	/// </summary>
	[Rpc(RpcSources.All, RpcTargets.StateAuthority)] // will only be invoked by the object that has state authority
	public void Rpc_Despawn()
	{
		Runner.Despawn(Object);
	}

	/// <summary>
	/// Fancy logging function.
	/// </summary>
	/// <param name="message">Message to display</param>
	/// <param name="ctx">Object to highlight in the UnityEditor</param>
	private void Log(string message, Object ctx = null) => Debug.Log(string.Join('\n', message.Split('\n').Select(s => $"<color=#8080ff>{s}</color>")), ctx);

	/// <summary>
	/// Changes the isAuthorityChanging flag when the state authority changes.
	/// </summary>
	public void StateAuthorityChanged()
	{
		if (isAuthorityChanging)
		{
			Log("authority changed.");
			isAuthorityChanging = false;
		}
	}
	
	/*
	 * Ensures that a pending authority request is handled.
	 * Confirms that all AuthorityHandler components in the hierarchy have state authority before proceeding.
	 * Once the conditions are met, the onAuthorized action is invoked.
	 *
	 * Used here to handle the authorization process smoothly and consistently within the networked environment.
	 */
	public override void FixedUpdateNetwork()
	{
		// If an authority request is pending
		if (onAuthorized != null)
		{
			// If all AuthorityHandler components in the hierarchy have state authority
			if (GetComponentsInChildren<AuthorityHandler>().All(h => h.Object.HasStateAuthority))
			{
				Log("invoking authority action");
				onAuthorized();
				onAuthorized = null;
				onUnauthorized = null;
				timeRequested = 0;
			}
		}
	}

	/*
	 * Handles timeouts for pending authority requests.
	 * Timeout is handled as unauthorized.
	 *
	 * Runs after all simulation updates have been processed (including Fusion).
	 */
	public override void Render()
	{
		bool hasPendingAuthorityRequest = timeRequested > 0;
		bool requestTimedOut = Runner.SimulationTime - timeRequested > 2;
		
		if (hasPendingAuthorityRequest && requestTimedOut)
		{
			Debug.LogWarning("timed out");
			timeRequested = 0;
			onUnauthorized?.Invoke();
			onAuthorized = null;
			onUnauthorized = null;
		}
	}
}