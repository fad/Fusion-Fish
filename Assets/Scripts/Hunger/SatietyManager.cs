using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SatietyManager : NetworkBehaviour, ISatietyManager
{
    private PlayerManager playerManager;

    [SerializeField]private int maxSatiety = 100;
    [SerializeField]private int satietyDecreaseRate = 1;

    private int _currentSatiety;
    
    public override void Spawned()
    {
        playerManager = GetComponent<PlayerManager>();
        Restart();
    }
    public void Restart()
    {
        _currentSatiety = maxSatiety;
        StartCoroutine(SatietyDecrease());
    } 
    public void Death()
    {
        StopAllCoroutines();
    }

    public void RecoverySatiety(float SatietyCount)
    {
        _currentSatiety += (int)SatietyCount;

        if (maxSatiety < _currentSatiety)
            _currentSatiety = maxSatiety;
    }
    private IEnumerator SatietyDecrease()
    {
        while (true)
        {

            if (!playerManager.levelUp.isEgg)
                _currentSatiety -= satietyDecreaseRate;

            if (_currentSatiety <= 0)
            {
                playerManager.playerHealth.causeOfDeath = "You starved";
                playerManager.healthManager.ReceiveDamageRpc(playerManager.healthManager.maxHealth);
                break;
            }

            yield return new WaitForSeconds(1f);
        }
    }
    public float GetSatiety()
    {
        return _currentSatiety;
    }
    public float GetMaxSatiety()
    {
        return  maxSatiety;
    }
    public void UpdateSatietyData(int maxSatiety, int satietyDecreaseRate)
    {
        if(maxSatiety > 0)
        {
            this.maxSatiety = maxSatiety;
            _currentSatiety = maxSatiety;
        }
        if(satietyDecreaseRate > 0)
            this.satietyDecreaseRate = satietyDecreaseRate;
    }
    
}
