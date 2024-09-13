using BiggestFish.Gameplay;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;

public class Attack : NetworkBehaviour
{
    [Header("Food")] 
    [SerializeField] private LayerMask foodLayerMask;
    private GameObject foodObject;

    [Header("Bite")] 
    [SerializeField] private Transform biteUpper;
    [SerializeField] private Transform biteLower;
    [SerializeField] private Animator biteAnimator;
    
    [Header("Attack")] 
    public float attackDamage = 1;
    private bool preparedAttack;
    
    [Header("Time")]
    private float timeBetweenAttack;
    [SerializeField] private float maxTimeBetweenAttack = 0.375f;

    [Header("Player")]
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private Transform fishRender;
    private float maxSensitivity;
    private float halfSensitivity;

    [Header("SuckIn")] 
    [SerializeField] private ParticleSystem suckInParticleSystem;
    [SerializeField] private float suckInDelay = 1;
    [SerializeField] private float currentSuckInDelay = 1;
    [SerializeField] public float suckPower;

    private void Start()
    {
        maxSensitivity = thirdPersonController.sensitivity;
        halfSensitivity = thirdPersonController.sensitivity /= 2;
        
        timeBetweenAttack = maxTimeBetweenAttack;
    }

    private void Update()
    {
        AttackUpdate();
        
        SuckInUpdate();
    }

    private void SuckInUpdate()
    {
        currentSuckInDelay -= Time.deltaTime;

        if (thirdPersonController.input.suckIn)
        {
            if (currentSuckInDelay <= 0)
            {
                suckInParticleSystem.Play();

                if (foodObject != null)
                {
                    if (foodObject.GetComponent<Health>().NetworkedHealth <= suckPower && !foodObject.GetComponent<NPC>())
                    {
                        thirdPersonController.playerManager.experience.currentExperience += foodObject.GetComponent<Health>().experienceValue;
                        foodObject.GetComponent<Health>().ReceiveDamageRpc(suckPower);
                    }
                }

                currentSuckInDelay = suckInDelay;
            }
        }
    }
    
    private void AttackUpdate()
    {
        if (thirdPersonController.playerManager.health.isDead)
            return;

        if (timeBetweenAttack >= 0)
        {
            timeBetweenAttack -= Time.deltaTime;
            return;
        }
        
        switch (thirdPersonController.input.attack)
        {
            case true when !preparedAttack:
                biteUpper.gameObject.SetActive(true);
                biteLower.gameObject.SetActive(true);
                biteAnimator.SetTrigger("prepareAttack");
                thirdPersonController.sensitivity = halfSensitivity;
                preparedAttack = true;
                break;
            case false when preparedAttack:
                biteAnimator.SetTrigger("executeAttack");
                thirdPersonController.animator.SetTrigger("attack");
                timeBetweenAttack = maxTimeBetweenAttack;
                thirdPersonController.sensitivity = maxSensitivity;
                preparedAttack = false;
                break;
        }
    }

    public void ResetBiteImageAnimationEvent()
    {
        biteUpper.gameObject.SetActive(false);
        biteLower.gameObject.SetActive(false);
    }
    
    private void DamageAnimationEvent()
    {
        if (foodObject != null && !foodObject.GetComponent<MeatObject>())
        {
            foodObject.GetComponent<Health>().ReceiveDamageRpc(attackDamage);
            if (foodObject.GetComponent<Health>().NetworkedHealth <= 0)
            {
                biteUpper.GetComponent<Image>().color = Color.white;
                biteLower.GetComponent<Image>().color = Color.white;
            }
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if ((1 << col.gameObject.layer) == foodLayerMask.value)
        {
            foodObject = col.GetComponent<Transform>().transform.gameObject;
            biteUpper.GetComponent<Image>().color = Color.yellow;
            biteLower.GetComponent<Image>().color = Color.yellow;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if ((1 << col.gameObject.layer) == foodLayerMask.value)
        {
            foodObject = null;
            biteUpper.GetComponent<Image>().color = Color.white;
            biteLower.GetComponent<Image>().color = Color.white;
        }
    }
}
