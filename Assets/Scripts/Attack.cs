using System;
using BiggestFish.Gameplay;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class Attack : NetworkBehaviour
{
    [Header("Food")] 
    [SerializeField] private LayerMask foodLayerMask;
    [SerializeField] private LayerMask playerLayerMask;
    private GameObject foodObject;

    [Header("Bite")] 
    [SerializeField] private Transform biteUpper;
    [SerializeField] private Transform biteLower;
    [SerializeField] private Animator biteAnimator;
    
    [Header("Attack")] 
    public float attackDamage = 1;
    private bool preparedAttack;
    [SerializeField] private Transform attackPosition;
    [SerializeField] private float attackRange;
    
    [Header("Time")]
    private float timeBetweenAttack;
    [SerializeField] private float maxTimeBetweenAttack = 0.375f;

    [Header("Player")]
    [SerializeField] private ThirdPersonController thirdPersonController;
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
                
                AudioManager.Instance.PlaySoundWithRandomPitchAtPosition("suck", transform.position);
                
                if (foodObject != null)
                {
                    if (foodObject.GetComponent<Health>().maxHealth <= suckPower)
                    {
                        thirdPersonController.playerManager.experience.currentExperience += foodObject.GetComponent<Health>().experienceValue;
                        foodObject.GetComponent<Health>().ReceiveDamageRpc(suckPower, false);
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
        
        EnemyInRange();

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
                thirdPersonController.networkAnimator.SetTrigger("attack");
                timeBetweenAttack = maxTimeBetweenAttack;
                thirdPersonController.sensitivity = maxSensitivity;
                preparedAttack = false;
                break;
        }
    }

    private void EnemyInRange()
    {
        var hitColliders = new Collider[1];
        var foodHits = Physics.OverlapSphereNonAlloc(attackPosition.position, attackRange, hitColliders, foodLayerMask);
        var playerHits = Physics.OverlapSphereNonAlloc(attackPosition.position, attackRange, hitColliders, playerLayerMask);
        
        if (foodHits >= 1 || playerHits >= 1)
        {
            foodObject = hitColliders[0].transform.gameObject;
            biteUpper.GetComponent<Image>().color = Color.yellow;
            biteLower.GetComponent<Image>().color = Color.yellow;   
        }
        else
        {
            foodObject = null;
            biteUpper.GetComponent<Image>().color = Color.white;
            biteLower.GetComponent<Image>().color = Color.white;   
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
            foodObject.GetComponent<Health>().ReceiveDamageRpc(attackDamage, true);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(attackPosition.position, attackRange);
    }
}
