using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class PlayerAttack : NetworkBehaviour
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
    [SerializeField] public float attackRange;
    [SerializeField] private float maxTimeBetweenAttack = 0.375f;
    [SerializeField] private float attractionAngle;
    public float attackDamage = 1;
    private float currentAttackTime;
    private bool preparedAttack;
    
    [Header("Player")]
    [SerializeField] private PlayerManager playerManager;
    private float usualSensitivity;
    private float sensitivityWhileAttacking;

    [Header("SuckIn")] 
    [SerializeField] private ParticleSystem suckInParticles;
    [SerializeField] private float maxTimeBetweenSuckIn = 1;
    [SerializeField] private float suckInForce;
    [SerializeField] private float healthIncreaseOnEating = 10;
    public float suckInDamage;
    private float currentSuckInTime;

    private void Start()
    {
        if (!HasStateAuthority)
            return;
        
        playerManager.thirdPersonController.gameObject.layer = LayerMask.NameToLayer("StateAuthorityPlayer");

        usualSensitivity = playerManager.thirdPersonController.sensitivity;
        sensitivityWhileAttacking = playerManager.thirdPersonController.sensitivity /= 2;
        
        currentAttackTime = maxTimeBetweenAttack;
    }

    private void Update()
    {
        if (!HasStateAuthority || playerManager.playerHealth.isDead) 
            return;
        
        AttackUpdate();
        
        SuckInUpdate();
        
        EnemyInRange();
    }

    private void SuckInUpdate()
    {
        if (currentSuckInTime >= 0)
        {
            currentSuckInTime -= Time.deltaTime;
        }
        else if (playerManager.thirdPersonController.input.suckIn)
        {
            var playerVisualPosition = playerManager.thirdPersonController.playerVisual.transform.position;
                
            suckInParticles.Play();
            AudioManager.Instance.PlaySoundWithRandomPitchAtPosition("suck", playerVisualPosition);
            
            var hitColliders = new Collider[5];

            var hits = Physics.OverlapSphereNonAlloc(playerVisualPosition, attackRange, hitColliders, foodLayerMask);
            
            for (var i = 0; i < hits; i++)
            {
                if (hitColliders[i].TryGetComponent<HealthManager>(out var health) && health.maxHealth <= suckInDamage)
                {
                    // Calculate the direction from this object to the target
                    var directionToTarget = hitColliders[i].transform.position - playerManager.thirdPersonController.playerVisual.transform.position;

                    var angleToTarget = Vector3.Angle(-playerManager.thirdPersonController.playerVisual.transform.forward, directionToTarget);

                    // Check if the target is within the attraction distance and angle
                    if (angleToTarget <= attractionAngle)
                    {
                        // Check if the target is within the attraction distance and angle
                        // Apply attraction force to the target
                        if(hitColliders[i].TryGetComponent<Rigidbody>(out var targetRb))
                        {
                            targetRb.AddForce(directionToTarget.normalized * -suckInForce, ForceMode.Force);
                        }
                    }

                    //Decreasing lossyScale cause it is a bit too big
                    if (directionToTarget.magnitude <= playerManager.thirdPersonController.transform.localToWorldMatrix.lossyScale.z - .01f)
                    {
                        playerManager.levelUp.currentExperience += health.experienceValue;
                        playerManager.levelUp.CheckLevelUp();

                        if (playerManager.healthManager.NetworkedHealth + healthIncreaseOnEating <= playerManager.healthManager.maxHealth)
                        {
                            playerManager.healthManager.NetworkedHealth += healthIncreaseOnEating;
                        }
                        else
                        {
                            playerManager.healthManager.NetworkedHealth = playerManager.healthManager.maxHealth;
                        }

                        //decreasing experience value to 0, to make sure I do not get experience twice
                        health.experienceValue = 0;
                        health.ReceiveDamageRpc(suckInDamage, false);
                    }   
                }
            }

            currentSuckInTime = maxTimeBetweenSuckIn;
        }
    }
    
    private void AttackUpdate()
    {
        if (currentAttackTime >= 0)
        {
            currentAttackTime -= Time.deltaTime;
            return;
        }
        
        switch (playerManager.thirdPersonController.input.attack)
        {
            case true when !preparedAttack:
                biteUpper.gameObject.SetActive(true);
                biteLower.gameObject.SetActive(true);
                biteAnimator.SetTrigger("prepareAttack");
                playerManager.thirdPersonController.sensitivity = sensitivityWhileAttacking;
                preparedAttack = true;
                break;
            case false when preparedAttack:
                biteAnimator.SetTrigger("executeAttack");
                playerManager.thirdPersonController.animator.SetTrigger("attack");
                currentAttackTime = maxTimeBetweenAttack;
                playerManager.thirdPersonController.sensitivity = usualSensitivity;
                preparedAttack = false;
                break;
        }
    }

    private void EnemyInRange()
    {
        var hitColliders = new Collider[1];

        var playerVisualPosition = playerManager.thirdPersonController.playerVisual.transform.position;
        var foodHits = Physics.OverlapSphereNonAlloc(playerVisualPosition, attackRange, hitColliders, foodLayerMask);
        var playerHits = Physics.OverlapSphereNonAlloc(playerVisualPosition, attackRange, hitColliders, playerLayerMask);
        
        if (foodHits >= 1 || playerHits >= 1)
        {
            var directionToTarget = hitColliders[0].transform.position - playerVisualPosition;
            var angleToTarget = Vector3.Angle(-playerManager.thirdPersonController.playerVisual.transform.forward, directionToTarget);
            
            // Check if the target is within the attraction angle
            if (angleToTarget <= attractionAngle && !hitColliders[0].GetComponent<HealthManager>().notAbleToGetBitten)
            {
                SetFoodObject(hitColliders[0].transform.gameObject, Color.yellow);
            }
            else
            {
                SetFoodObject(null, Color.white);
            }
        }
    }

    private void SetFoodObject(GameObject food, Color color)
    {
        foodObject = food;
        biteUpper.GetComponent<Image>().color = color;
        biteLower.GetComponent<Image>().color = color;
    }
    
    public void ResetBiteImageAnimationEvent()
    {
        biteUpper.gameObject.SetActive(false);
        biteLower.gameObject.SetActive(false);
        SetFoodObject(null, Color.white);
    }
    
    private void DamageAnimationEvent()
    {
        if (foodObject != null && foodObject.TryGetComponent<HealthManager>(out var health))
        {
            health.ReceiveDamageRpc(attackDamage, true);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerManager.thirdPersonController.playerVisual.transform.position, attackRange);
    }
}
