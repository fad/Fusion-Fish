using StarterAssets;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth;
    [HideInInspector] public float currentHealth;
    private ParticleSystem bloodParticleSystem;
    private ThirdPersonController thirdPersonController;
    private NPC npc;
    private bool isNPC;
    private bool isPlayer;

    [Header("Experience")] 
    public int experienceValue = 100;
    
    [Header("Death")]
    [HideInInspector] public bool isDead;
    public GameObject deathPanel;

    private void Start()
    {
        if (TryGetComponent(out ThirdPersonController thirdPersonControllerTemp))
        {
            isPlayer = true;
            thirdPersonController = thirdPersonControllerTemp;
        }

        if (TryGetComponent(out NPC npcTemp))
        {
            npc = npcTemp;
            isNPC = true;
        }
        currentHealth = maxHealth;
        bloodParticleSystem = FindObjectOfType<ParticleSystem>();
    }

    public void ReceiveDamage(int damage)
    {
        currentHealth -= damage;

        if (isNPC)
        {
            PlayParticles(Color.red, 10);
        }
        
        if (currentHealth <= 0)
        {
            if (isPlayer)
            {
                Die();
                return;
            }
            
            if (isNPC)
            {
                PlayParticles(Color.red, 30);
            }
            else
            {
                PlayParticles(Color.yellow, 10);
            }
        }
    }

    private void Die()
    {
        if (isPlayer)
        {
            thirdPersonController.animator.SetBool("isDead", true);
            isDead = true;
            deathPanel.SetActive(true);
        }
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
