using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DecalController : MonoBehaviour
{
    [SerializeField]private GameObject[] decals;
    private readonly List<GameObject> _activeDecals = new();
    private HealthManager _healthManager;
    private int _decalsCount;

    private void Start()
    {
        _healthManager = GetComponentInParent<HealthManager>();
        _healthManager.OnHealthChanged += ChekHealth;
    }

    private void OnDisable()
    {
        _healthManager.OnHealthChanged -= ChekHealth;
    }

    private void ChekHealth(float health)
    {
        float nextStage = _healthManager.maxHealth - ((_healthManager.maxHealth * 20 / 100) * (_decalsCount+1));
        float oldStage = _healthManager.maxHealth - ((_healthManager.maxHealth * 20 / 100) * _decalsCount);

        if(nextStage > health && _decalsCount < decals.Length && health>0)
        {
            NewDecal();
            _decalsCount++;
        }
        else if(oldStage < health && _decalsCount>0)
        {
            GameObject lastDecal = _activeDecals.LastOrDefault();
            lastDecal.SetActive(false);
            _activeDecals.Remove(lastDecal);
            _decalsCount --;
        }
    }
    private void NewDecal()
    {
        int number;
        do
        {
            number = Random.Range(0, decals.Length);
        } while (decals[number].activeInHierarchy);
        
        decals[number].SetActive(true);
        _activeDecals.Add(decals[number]);
    }
}
