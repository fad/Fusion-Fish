using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DecalController : MonoBehaviour
{
    [SerializeField]private GameObject[] decals;
    private List<GameObject> activeDecals;
    private HealthManager healthManager;
    private int decalsCount;

    private void Start()
    {
        healthManager = GetComponentInParent<HealthManager>();
        healthManager.OnHealthChanged += ChekHealth;
    }

    private void OnDisable()
    {
        healthManager.OnHealthChanged -= ChekHealth;
    }

    private void ChekHealth(float health)
    {
        float NextStage = healthManager.maxHealth - ((healthManager.maxHealth * 20 / 100) * (decalsCount+1));
        float OldStage = healthManager.maxHealth - ((healthManager.maxHealth * 20 / 100) * decalsCount);

        if(NextStage > health && decalsCount < decals.Length && health>0)
        {
            NewDecal();
            decalsCount++;
        }
        else if(OldStage < health && decalsCount>0)
        {
            GameObject lastDecal = activeDecals.LastOrDefault();
            lastDecal.SetActive(false);
            activeDecals.Remove(lastDecal);
            decalsCount --;
        }
    }
    private void NewDecal()
    {
        int number;
        while(true)
        {
            number = Random.Range(0,4);
            if(!decals[number].activeInHierarchy)
                break;
        }
        decals[number].SetActive(true);
        activeDecals.Add(decals[number]);
    }
}
