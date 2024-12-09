using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using StarterAssets;

public class PlayerAttack : NetworkBehaviour
{
    [Header("Hit LayerMask")]
    [SerializeField]
    private LayerMask foodLayerMask;

    [SerializeField]
    private LayerMask hittableLayerMask;

    private GameObject _foodObject;

    [Header("Bite")]
    [SerializeField]
    private Transform biteUpper;

    [SerializeField]
    private Transform biteLower;

    [SerializeField]
    private Animator biteAnimator;

    [SerializeField]
    private NetworkMecanimAnimator networkFishAnimator;

    [Header("Attack")]
    [SerializeField]
    public float attackRange;

    [SerializeField]
    private float maxTimeBetweenAttack = 0.375f;

    [SerializeField]
    private float attractionAngle;

    public float attackDamage = 1;
    private float _currentAttackTime;
    private bool _preparedAttack;
    private bool _sucksFood;
    float suctionInterval = 0.3f;
    float currentSuctionInterval = 0;
    private int _attackCount;
    private int _lastAttackCount;

    [Header("Player")]
    [SerializeField]
    private PlayerManager playerManager;

    private float _usualSensitivity;
    private float _sensitivityWhileAttacking;

    [Header("SuckIn")]
    [SerializeField]
    private ParticleSystem suckInParticles;

    [SerializeField]
    private float suckInForce;

    [SerializeField]
    private float healthIncreaseOnEating = 10;

    [SerializeField]
    private int satietyIncreaseOnEating = 10;

    private bool _scaleUpAnimationRunning;

    [Tooltip("Increase player scale up and down coroutine will take that time.")]
    [SerializeField]
    private float timeForScaleAnimation = .15f;

    public float suckInDamage;

    private Image _biteUpperImage;
    private Image _biteLowerImage;

    private OutlineManager _currentEnemyOutline;
    private HealthViewModel _currentEnemyHealthBar;

    private static readonly int PrepareAttack = Animator.StringToHash("prepareAttack");
    private static readonly int ExecuteAttack = Animator.StringToHash("executeAttack");


    private void Start()
    {
        if (!HasStateAuthority)
            return;

        playerManager.thirdPersonController.gameObject.layer = LayerMask.NameToLayer("StateAuthorityPlayer");

        _usualSensitivity = playerManager.thirdPersonController.sensitivity;
        _sensitivityWhileAttacking = playerManager.thirdPersonController.sensitivity /= 2;

        _currentAttackTime = maxTimeBetweenAttack;

        _biteUpperImage = biteUpper.GetComponent<Image>();
        _biteLowerImage = biteLower.GetComponent<Image>();
    }

    private void Update()
    {
        if (playerManager.levelUp.isEgg || !HasStateAuthority || playerManager.playerHealth.isDead)
            return;

        if (!_sucksFood)
            AttackUpdate();

        SuckInUpdate();

        EnemyInRange();
    }

    private void SuckInUpdate()
    {
        if (playerManager.thirdPersonController.input.suckIn)
        {
            Vector3 playerVisualPosition = playerManager.thirdPersonController.playerVisual.transform.position;
            _sucksFood = true;
            if (!suckInParticles.isPlaying)
                suckInParticles.Play();
            AudioManager.Instance.PlaySoundWithRandomPitchAtPosition("suck", playerVisualPosition);

            currentSuctionInterval -= Time.deltaTime;

            if (currentSuctionInterval <= 0)
            {
                Collider[] hitColliders = new Collider[5];

                int hits = Physics.OverlapSphereNonAlloc(playerVisualPosition, attackRange, hitColliders, foodLayerMask);

                for (int i = 0; i < hits; i++)
                {
                    if (hitColliders[i].TryGetComponent(out ISuckable suckable) &&
                        suckable.NeededSuckingPower <= suckInDamage)
                    {
                        if (TryGetComponent<PlayerHealth>(out var playerHealth) && playerHealth.NetworkedPermanentHealth)
                            return;

                        CalculateDirectionAndAddForceTowardsPlayer(hitColliders[i], out Vector3 directionToTarget);
                        ApplyExperienceAndResetFoodObject(directionToTarget, suckable);
                    }
                }

                currentSuctionInterval = suctionInterval;
            }
        }
        else
        {
            _sucksFood = false;
            suckInParticles.Stop();
        }
    }

    private void CalculateDirectionAndAddForceTowardsPlayer(Collider hitCollider, out Vector3 directionToTarget)
    {
        // Calculate the direction from this object to the target
        directionToTarget = hitCollider.transform.position -
                            playerManager.thirdPersonController.playerVisual.transform.position;

        float angleToTarget = Vector3.Angle(-playerManager.thirdPersonController.playerVisual.transform.forward,
            directionToTarget);

        // Check if the target is within the attraction angle
        if (angleToTarget <= attractionAngle)
        {
            // Apply force to the target
            if (hitCollider.TryGetComponent<Rigidbody>(out var targetRb))
            {
                if (hitCollider.TryGetComponent<NPCBehaviour>(out var npcBehaviour))
                    StartCoroutine(npcBehaviour.StopMovement(2));

                targetRb.AddForce(directionToTarget.normalized * -suckInForce, ForceMode.Force);
            }
        }
    }

    private void ApplyExperienceAndResetFoodObject(Vector3 directionToTarget, ISuckable suckable)
    {
        //Decreasing lossyScale because it is a bit too big
        if (directionToTarget.magnitude <=
            playerManager.thirdPersonController.transform.localToWorldMatrix.lossyScale.z - .01f)
        {
            playerManager.levelUp.AddExperience(suckable.GetSuckedIn());

            if (!_scaleUpAnimationRunning)
                StartCoroutine(ScalePlayerUpOnEating());

            playerManager.healthManager.RecoveryHealthRpc(healthIncreaseOnEating);
            playerManager.satietyManager.RecoverySatiety(satietyIncreaseOnEating);

            SetFoodObject(null, Color.white, true);
        }
    }

    private IEnumerator ScalePlayerUpOnEating()
    {
        _scaleUpAnimationRunning = true;

        var oldScale = playerManager.transform.localScale;
        var scaleUpFish = oldScale + Vector3.one / 5;
        var elapsedTime = 0f;

        while (elapsedTime < timeForScaleAnimation)
        {
            playerManager.transform.localScale = Vector3.Lerp(playerManager.transform.localScale, scaleUpFish,
                elapsedTime / timeForScaleAnimation);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;

        while (elapsedTime < timeForScaleAnimation)
        {
            playerManager.transform.localScale = Vector3.Lerp(playerManager.transform.localScale, oldScale,
                elapsedTime / timeForScaleAnimation);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _scaleUpAnimationRunning = false;
    }

    private void AttackUpdate()
    {
        if (_currentAttackTime >= 0)
        {
            _currentAttackTime -= Time.deltaTime;
            return;
        }

        switch (playerManager.thirdPersonController.input.attack)
        {
            case true when !_preparedAttack:
                biteUpper.gameObject.SetActive(true);
                biteLower.gameObject.SetActive(true);
                biteAnimator.SetTrigger(PrepareAttack);
                playerManager.thirdPersonController.sensitivity = _sensitivityWhileAttacking;
                _preparedAttack = true;
                break;
            case false when _preparedAttack:
                biteAnimator.SetTrigger(ExecuteAttack);
                _attackCount++;
                _currentAttackTime = maxTimeBetweenAttack;
                playerManager.thirdPersonController.sensitivity = _usualSensitivity;
                _preparedAttack = false;
                break;
        }
    }

    public override void Render()
    {
        if (_attackCount > _lastAttackCount)
        {
            networkFishAnimator.SetTrigger("attack");
        }

        _lastAttackCount = _attackCount;
    }

    private void EnemyInRange()
    {
        Collider[] hitColliders = new Collider[1];

        Vector3 playerVisualPosition = playerManager.thirdPersonController.playerVisual.transform.position;
        int hits = Physics.OverlapSphereNonAlloc(playerVisualPosition, attackRange, hitColliders, hittableLayerMask);

        if (hits >= 1)
        {
            Vector3 directionToTarget = hitColliders[0].transform.position - playerVisualPosition;
            float angleToTarget = Vector3.Angle(-playerManager.thirdPersonController.playerVisual.transform.forward,
                directionToTarget);

            HealthManager health = hitColliders[0].GetComponentInChildren<HealthManager>();

            // Check if the target is within the attraction angle
            if (angleToTarget <= attractionAngle && health && !health.notAbleToGetBitten)
            {
                SetFoodObject(hitColliders[0].transform.gameObject, Color.yellow, false);

                if (hitColliders[0].TryGetComponent(out _currentEnemyOutline))
                {
                    _currentEnemyOutline.ShouldOutline(true);
                }

                if (hitColliders[0].TryGetComponent(out _currentEnemyHealthBar))
                {
                    _currentEnemyHealthBar.AdjustHealthBarVisibility(true);
                }
            }
            else
            {
                SetFoodObject(null, Color.white, true);
            }
        }
        else
        {
            SetFoodObject(null, Color.white, true);
        }
    }


    /// <summary>
    /// Sets the food object and updates the UI elements accordingly.
    /// </summary>
    /// <param name="food">The food GameObject to set.</param>
    /// <param name="color">The color to apply to the bite images.</param>
    /// <param name="deactivateEnemyUI">If true, deactivates the enemy UI elements.</param>
    private void SetFoodObject(GameObject food, Color color, bool deactivateEnemyUI)
    {
        _foodObject = food;
        _biteUpperImage.color = color;
        _biteLowerImage.color = color;

        if (deactivateEnemyUI)
        {
            if (_currentEnemyOutline)
                _currentEnemyOutline.ShouldOutline(false);

            if (_currentEnemyHealthBar)
            {
                _currentEnemyHealthBar.AdjustHealthBarVisibility(false);

                _currentEnemyHealthBar = null;
            }
        }
    }

    public void ResetBiteImageAnimationEvent()
    {
        biteUpper.gameObject.SetActive(false);
        biteLower.gameObject.SetActive(false);

        SetFoodObject(null, Color.white, false);
    }

    private void DamageAnimationEvent()
    {
        if (_foodObject != null && _foodObject.TryGetComponent<HealthManager>(out var health))
        {
            if (_foodObject.TryGetComponent<PlayerHealth>(out var playerHealth) &&
                playerHealth.NetworkedPermanentHealth)
            {
                playerHealth.causeOfDeath = "You got eaten";
                return;
            }

            if (_foodObject.TryGetComponent(out HealthManager healthM) && healthM.notAbleToGetBitten)
                return;

            health.ReceiveDamageRpc(attackDamage);

            if (_foodObject.TryGetComponent<ThirdPersonController>(out var player))
                player.GraspedRpc(playerManager.GetComponent<NetworkTransform>());

            playerManager.thirdPersonController.StartAttractToEntity(_foodObject.transform);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerManager.thirdPersonController.playerVisual.transform.position, attackRange);
    }
}
