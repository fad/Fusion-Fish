using Fusion;
using StarterAssets;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [Header("Health")]
    public float maxHealth;
    [HideInInspector] public float currentHealth;
    private ParticleSystem bloodParticleSystem;
    private ThirdPersonController thirdPersonController;
    private bool isPlayer;

    [Header("Experience")] 
    public int experienceValue = 100;
    
    [Header("Death")]
    [HideInInspector] public bool isDead;
    [SerializeField] private Transform gibs;
    public GameObject deathPanel;

    private void Start()
    {
        if (TryGetComponent(out ThirdPersonController thirdPersonControllerTemp))
        {
            isPlayer = true;
            thirdPersonController = thirdPersonControllerTemp;
        }
        
        currentHealth = maxHealth;
        bloodParticleSystem = GameObject.Find("BloodParticles").GetComponent<ParticleSystem>();
    }

    public void ReceiveDamage(float damage)
    {
        currentHealth -= damage;

        PlayParticles(Color.red, 10);
        
        if (currentHealth <= 0)
        {
            if (isPlayer)
            {
                PlayerDeath();
                PlayParticles(Color.red, 30);
            }
            else
            {
                PlayParticles(Color.red, 20);
                Runner.Despawn(GetComponent<NetworkObject>());
            }
        }
    }

    private void PlayerDeath()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        thirdPersonController.playerMesh.SetActive(false);
        for (var i = 0; i < 4; i++)
        {
            var food = Instantiate(gibs);
            food.transform.position = transform.position;
        }
        isDead = true;
        deathPanel.SetActive(true);
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
