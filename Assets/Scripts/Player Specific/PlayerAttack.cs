using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class PlayerAttack : NetworkBehaviour
{
    [Header("Hit LayerMask")] 
    [SerializeField] private LayerMask foodLayerMask;
    [SerializeField] private LayerMask hittableLayerMask;
    private GameObject foodObject;

    [Header("Bite")] 
    [SerializeField] private Transform biteUpper;
    [SerializeField] private Transform biteLower;
    [SerializeField] private Animator biteAnimator;
    [SerializeField] private NetworkMecanimAnimator networkFishAnimator;

    [Header("Attack")] 
    [SerializeField] public float attackRange;
    [SerializeField] private float maxTimeBetweenAttack = 0.375f;
    [SerializeField] private float attractionAngle;
    public float attackDamage = 1;
    private float currentAttackTime;
    private bool preparedAttack;
    private int attackCount;
    private int lastAttackCount;
    
    [Header("Player")]
    [SerializeField] private PlayerManager playerManager;
    private float usualSensitivity;
    private float sensitivityWhileAttacking;

    [Header("SuckIn")] 
    [SerializeField] private ParticleSystem suckInParticles;
    [SerializeField] private float maxTimeBetweenSuckIn = 1;
    [SerializeField] private float suckInForce;
    [SerializeField] private float healthIncreaseOnEating = 10;
    private bool scaleUpAnimationRunning;
    [Tooltip("Increase player scale up and down coroutine will take that time.")]
    [SerializeField] private float timeForScaleAnimation = .15f; 
    public float suckInDamage;
    private float _currentSuckInTime;
    
    private Image _biteUpperImage;
    private Image _biteLowerImage;

    private void Start()
    {
        if (!HasStateAuthority)
            return;
        
        playerManager.thirdPersonController.gameObject.layer = LayerMask.NameToLayer("StateAuthorityPlayer");

        usualSensitivity = playerManager.thirdPersonController.sensitivity;
        sensitivityWhileAttacking = playerManager.thirdPersonController.sensitivity /= 2;
        
        currentAttackTime = maxTimeBetweenAttack;
        
        _biteUpperImage = biteUpper.GetComponent<Image>();
        _biteLowerImage = biteLower.GetComponent<Image>();
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
        if (_currentSuckInTime >= 0)
        {
            _currentSuckInTime -= Time.deltaTime;
        }
        else if (playerManager.thirdPersonController.input.suckIn)
        {
            Vector3 playerVisualPosition = playerManager.thirdPersonController.playerVisual.transform.position;
                
            suckInParticles.Play();
            AudioManager.Instance.PlaySoundWithRandomPitchAtPosition("suck", playerVisualPosition);
            
            Collider[] hitColliders = new Collider[5];

            int hits = Physics.OverlapSphereNonAlloc(playerVisualPosition, attackRange, hitColliders, foodLayerMask);
            
            for (int i = 0; i < hits; i++)
            {
                if (hitColliders[i].TryGetComponent<HealthManager>(out var health) && health.maxHealth <= suckInDamage)
                {
                    if(TryGetComponent<PlayerHealth>(out var playerHealth) && playerHealth.NetworkedPermanentHealth)
                        return;
                    
                    CalculateDirectionAndAddForceTowardsPlayer(hitColliders[i], out Vector3 directionToTarget);
                    ApplyExperienceAndResetFoodObject(directionToTarget, health);   
                }
            }

            _currentSuckInTime = maxTimeBetweenSuckIn;
        }
    }

    private void CalculateDirectionAndAddForceTowardsPlayer(Collider hitCollider, out Vector3 directionToTarget)
    {
        // Calculate the direction from this object to the target
        directionToTarget = hitCollider.transform.position - playerManager.thirdPersonController.playerVisual.transform.position;

        float angleToTarget = Vector3.Angle(-playerManager.thirdPersonController.playerVisual.transform.forward, directionToTarget);

        // Check if the target is within the attraction angle
        if (angleToTarget <= attractionAngle)
        {
            // Apply force to the target
            if(hitCollider.TryGetComponent<Rigidbody>(out var targetRb))
            {
                if (hitCollider.TryGetComponent<NPCBehaviour>(out var npcBehaviour))
                    StartCoroutine(npcBehaviour.StopMovement(2));
                                    
                targetRb.AddForce(directionToTarget.normalized * -suckInForce, ForceMode.Force);
            }
        }
    }

    private void ApplyExperienceAndResetFoodObject(Vector3 directionToTarget, HealthManager health)
    {
        //Decreasing lossyScale cause it is a bit too big
        if (directionToTarget.magnitude <= playerManager.thirdPersonController.transform.localToWorldMatrix.lossyScale.z - .01f)
        {
            playerManager.levelUp.currentExperience += health.experienceValue;
            playerManager.levelUp.CheckLevelUp();
                        
            if (!scaleUpAnimationRunning)
                StartCoroutine(ScalePlayerUpOnEating());
                        
            if (playerManager.healthManager.NetworkedHealth + healthIncreaseOnEating <= playerManager.healthManager.maxHealth)
            {
                playerManager.healthManager.NetworkedHealth += healthIncreaseOnEating;
            }
            else
            {
                playerManager.healthManager.NetworkedHealth = playerManager.healthManager.maxHealth;
            }

            //decreasing experience value to 0, to make sure not to apply experience twice
            health.experienceValue = 0;
            health.ReceiveDamageRpc(suckInDamage, false);
            SetFoodObject(null, Color.white);
        }  
    }

    private IEnumerator ScalePlayerUpOnEating()
    {
        scaleUpAnimationRunning = true;
        
        var oldScale = playerManager.transform.localScale;
        var scaleUpFish = oldScale + Vector3.one / 5;
        var elapsedTime = 0f;

        while (elapsedTime < timeForScaleAnimation)
        {
            playerManager.transform.localScale = Vector3.Lerp(playerManager.transform.localScale, scaleUpFish, elapsedTime / timeForScaleAnimation);
        
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        elapsedTime = 0;
        
        while (elapsedTime < timeForScaleAnimation)
        {
            playerManager.transform.localScale = Vector3.Lerp( playerManager.transform.localScale, oldScale, elapsedTime / timeForScaleAnimation);
        
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        scaleUpAnimationRunning = false;
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
                attackCount++;
                currentAttackTime = maxTimeBetweenAttack;
                playerManager.thirdPersonController.sensitivity = usualSensitivity;
                preparedAttack = false;
                break;
        }
    }

    public override void Render()
    {
        if (attackCount > lastAttackCount)
        {
            networkFishAnimator.SetTrigger("attack");
        }

        lastAttackCount = attackCount;
    }
    
    private void EnemyInRange()
    {
        Collider[] hitColliders = new Collider[1];

        Vector3 playerVisualPosition = playerManager.thirdPersonController.playerVisual.transform.position;
        int hits = Physics.OverlapSphereNonAlloc(playerVisualPosition, attackRange, hitColliders, hittableLayerMask);
        
        if (hits >= 1)
        {
            Vector3 directionToTarget = hitColliders[0].transform.position - playerVisualPosition;
            float angleToTarget = Vector3.Angle(-playerManager.thirdPersonController.playerVisual.transform.forward, directionToTarget);
            HealthManager health = hitColliders[0].GetComponent<HealthManager>();
            
            // Check if the target is within the attraction angle
            if (angleToTarget <= attractionAngle && !health.notAbleToGetBitten)
            {
                SetFoodObject(hitColliders[0].transform.gameObject, Color.yellow);
            }
            else
            {
                SetFoodObject(null, Color.white);
            }
        }
        else
        {
            SetFoodObject(null, Color.white);
        }
    }

    private void SetFoodObject(GameObject food, Color color)
    {
        foodObject = food;
        _biteUpperImage.color = color;
        _biteLowerImage.color = color;
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
            if(foodObject.TryGetComponent<PlayerHealth>(out var playerHealth) && playerHealth.NetworkedPermanentHealth)
                return;
            
            health.ReceiveDamageRpc(attackDamage, true);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerManager.thirdPersonController.playerVisual.transform.position, attackRange);
    }
}
