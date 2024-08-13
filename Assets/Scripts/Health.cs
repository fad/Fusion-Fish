using StarterAssets;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth;
    [HideInInspector] public float currentHealth;
    private ParticleSystem bloodParticleSystem;

    private void Start()
    {
        currentHealth = maxHealth;
        bloodParticleSystem = FindObjectOfType<ParticleSystem>();
    }

    /// <summary>
    /// Here I managed the incoming damage of the objects and the score that is obtained when killing a NPC or eating food.
    /// On top I made a particle system that simulates blood or food particles because I noticed the player feedback lacking.
    /// </summary>
    /// <param name="damage"></param>
    
    public void ReceiveDamage(int damage)
    {
        currentHealth -= damage;

        if (GetComponent<NPC>())
        {
            PlayParticles(Color.red, 10);
        }
        
        if (currentHealth <= 0)
        {
            if (gameObject.GetComponent<NPC>())
            {
                PlayParticles(Color.red, 30);
                Die(100);
            }
            else
            {
                PlayParticles(Color.yellow, 10);
                Die(10);
            }
        }
    }

    private void Die(int experience)
    {
        FindObjectOfType<Attack>().isAttacking = false;
        FindObjectOfType<Experience>().currentExperience += experience;
        Destroy(gameObject);
    }

    //Here I change burst count and color when needed
    private void PlayParticles(Color color, int burstCount)
    {
        var mainModule = bloodParticleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(color);
        
        var emissionModule = bloodParticleSystem.emission;
        emissionModule.SetBursts(new ParticleSystem.Burst[] { new(0.0f, burstCount) });
        
        bloodParticleSystem.transform.position = transform.position;
        bloodParticleSystem.Play();
    }
}
