using UnityEngine;
using Fusion;

public class SlowDownManager : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField]
    public float maxSlowDownSpeedTime = 5;

    private bool _slowDown;
    
    public bool CurrentlySlowedDown => _slowDown;
    
    [Networked] private TickTimer SlowDownCooldownTimer { get; set; }

    public override void FixedUpdateNetwork()
    {
        if(Runner is null) return;
        
        if (SlowDownCooldownTimer.ExpiredOrNotRunning(Runner))
        {
            _slowDown = false;
        }
    }
    
    public void SlowDown()
    {
        if(Runner is null) return;
        
        if (_slowDown)
        {
            return;
        }

        _slowDown = true;
        
        SlowDownCooldownTimer = TickTimer.CreateFromSeconds(Runner, maxSlowDownSpeedTime);
    }
}
