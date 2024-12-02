using System;
using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NPCBehaviour : NetworkBehaviour
{
    [Header("Movement")] 
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxSwimSpeed = 95f;
    [SerializeField] private float defaultSwimSpeed = 30f;
    [SerializeField] private bool isMainMenuObject;
    private float currentSpeed;
    private bool stopMovement;

    [Header("Layers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask foodLayer;
    [SerializeField] private LayerMask stateAuthorityPlayer;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    private Animator animator;

    [Header("Attack")] 
    [SerializeField] private bool attacksAnything;
    [Space(5)]
    [SerializeField] private float timeBetweenAttacksMax = 1.5f;
    [SerializeField] private float timeBetweenAttacksMin = .8f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float detectRange;
    [SerializeField] private float detectionAngle;
    [SerializeField] private float attackRange;
    private bool canPlayFoundEnemySound = true;
    private bool isAttacking;
    private GameObject enemy;

    [FormerlySerializedAs("maxFleeTime")]
    [FormerlySerializedAs("maxFlightDelayTime")]
    [Header("Flight")]
    [SerializeField] private float maxTimeUntilFlee;
    [FormerlySerializedAs("flightTime")] [SerializeField] private float fleeTime = 3f;
    private float currentTimeUntilFlee;
    
    [Header("Natural Behaviour")]
    [Tooltip("Time which the NPC is pausing movement.")]
    [SerializeField] private float minWaitTime;
    [SerializeField] private float maxWaitTime;
    [SerializeField] private float minTimeUntilChangeMoveDirection = 6f;
    [SerializeField] private float maxTimeUntilChangeMoveDirection = 10f;
    private Rigidbody rb;
    [Tooltip("The position where the NPC moves next.")]
    private Vector3 newPosition;
    [Tooltip("Time until the NPC changes direction of movement.")] 
    private float randomWaitTime;
    [Tooltip("Time until the NPC stops movement.")]
    private float randomMoveTime;

    private float checkObstacleDistance = 1.5f;

    private Behaviour behaviour = Behaviour.NaturalBehaviour;
    private enum Behaviour
    {
        NaturalBehaviour,
        Flee,
        Attack,
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(stopMovement)
            return;
        
        DoBehaviour();
        
        CheckForEnemies();
        
        UpdateFleeTime();
    }

    //flees in opposite direction of the player, otherwise goes back to the natural movement of the NPC
    //But if the NPC gets attacked it cannot flee nor go back in the usual movement
    private void DoBehaviour()
    {
        switch (behaviour)
        {
            case Behaviour.Flee:
                if (enemy != null)
                {
                    var swimDirectionAwayFromEnemy = enemy.transform.position - transform.position;
                
                    currentSpeed = maxSwimSpeed;
                
                    if (Physics.Raycast(transform.position, -transform.forward, out var hitFlight, 1, groundLayer))
                    {
                        swimDirectionAwayFromEnemy = new Vector3(hitFlight.normal.x, hitFlight.normal.y, hitFlight.normal.z);
                        swimDirectionAwayFromEnemy *= 5;
                    }
                    
                    MoveNPCInDirection(swimDirectionAwayFromEnemy, Quaternion.LookRotation(swimDirectionAwayFromEnemy));   
                }
                break;
            case Behaviour.NaturalBehaviour:
                canPlayFoundEnemySound = true;
                
                var currentPosition = transform.position;

                if (randomWaitTime <= 0)
                {
                    newPosition =  Random.insideUnitSphere;
                    randomWaitTime = Random.Range(minTimeUntilChangeMoveDirection, maxTimeUntilChangeMoveDirection);
                    randomMoveTime = Random.Range(minWaitTime, maxWaitTime);
                }

                if (randomMoveTime > 0)
                {
                    randomMoveTime -= Time.deltaTime;
                }
                else
                {
                    randomWaitTime -= Time.deltaTime;
                    currentSpeed = defaultSwimSpeed;

                    //Make NPC not swim toward objects
                    Debug.DrawRay(currentPosition, -transform.forward * checkObstacleDistance,Color.blue);
                    if (Physics.Raycast(currentPosition, -transform.forward, out var hit, checkObstacleDistance, groundLayer))
                    {
                        newPosition = hit.normal * -1;
                    }

                    MoveNPCInDirection(newPosition, Quaternion.LookRotation(newPosition));
                }
                break;
            case Behaviour.Attack:
                if (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) < 15 && !Physics.Raycast(transform.position, -transform.forward, 1, groundLayer))
                {
                    if (enemy.TryGetComponent<PlayerHealth>(out var playerHealth) && playerHealth.isDead)
                    {
                        behaviour = Behaviour.NaturalBehaviour;
                    }

                    if (playerHealth && canPlayFoundEnemySound && playerHealth.HasStateAuthority)
                    {
                        AudioManager.Instance.PlaySoundWithRandomPitchAtPosition("FishFoundYou", enemy.transform.position);
                        canPlayFoundEnemySound = false;
                    }
                    
                    if (Vector3.Distance(transform.position, enemy.transform.position) < attackRange)
                    {
                        if (!isAttacking)
                        {
                            StartCoroutine(AttackCoroutine());
                        }
                    }
                    else
                    {
                        var swimDirectionEnemy = transform.position - enemy.transform.position;

                        currentSpeed = maxSwimSpeed;
                
                        MoveNPCInDirection(swimDirectionEnemy, Quaternion.LookRotation(swimDirectionEnemy));   
                    }
                }
                else
                {
                    behaviour = Behaviour.NaturalBehaviour;
                }
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if(!isMainMenuObject)
            animator.SetFloat("movingSpeed", rb.velocity.sqrMagnitude * 3);
    }
    
    private void MoveNPCInDirection(Vector3 targetDirection, Quaternion lookDirection)
    {
        int slowdown  = 1;
        if (TryGetComponent<HealthManager>(out var health))
        {
            if(health.slowDown)
                slowdown = 2;
            if(health.grasped)
                slowdown = 10;
        }

        rb.AddForce(targetDirection.normalized * (-currentSpeed / slowdown * Time.deltaTime), ForceMode.Impulse);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookDirection, Time.deltaTime * rotationSpeed);
    }

    //Flees for a certain amount of time and goes back into normal movement behaviour
    private IEnumerator FleeCoroutine()
    {
        behaviour = Behaviour.Flee;

        yield return new WaitForSeconds(fleeTime);

        rb.velocity = Vector3.zero;
        currentTimeUntilFlee = maxTimeUntilFlee;
        behaviour = Behaviour.NaturalBehaviour;
    }

    private void UpdateFleeTime()
    {
        if (currentTimeUntilFlee > 0 && !attacksAnything)
        {
            currentTimeUntilFlee -= Time.deltaTime;
        }
    }
    
    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        
        yield return new WaitForSeconds(Random.Range(timeBetweenAttacksMin, timeBetweenAttacksMax));
        
        animator.SetTrigger("attack");
        
        yield return new WaitForSeconds(.2f);
        
        if(enemy != null && Vector3.Distance(transform.position, enemy.transform.position) < attackRange + .5f)
        {
            if (enemy.TryGetComponent<PlayerHealth>(out var playerHealth) && (playerHealth.isDead || playerHealth.NetworkedPermanentHealth))
            {
                isAttacking = false;
                playerHealth.causeOfDeath = "You got eaten";
                yield break;
            }
            //enemy.GetComponent<HealthManager>().ReceiveDamageRpc(attackDamage, true);
        }

        isAttacking = false;
    }

    //Checks if the player entered the vision of the NPC and flees when possible
    private void CheckForEnemies()
    {
        var hitColliders = new Collider[1];

        var position = transform.position;
        var foodHits = Physics.OverlapSphereNonAlloc(position, detectRange, hitColliders, foodLayer);
        var playerHits = Physics.OverlapSphereNonAlloc(position, detectRange, hitColliders, playerLayer);
        var playerStateAuthorityHits = Physics.OverlapSphereNonAlloc(position, detectRange, hitColliders, stateAuthorityPlayer);
        
        //Checks for two players because the player always detects itself first
        if ((foodHits >= 1 && transform.localScale.z > hitColliders[0].transform.localScale.z) || playerHits >= 1 || 
            playerStateAuthorityHits >= 1 && hitColliders[0].TryGetComponent<HealthManager>(out var health) && 
            !health.notAbleToGetBitten && health != GetComponent<HealthManager>())
        {
            if(hitColliders[0].TryGetComponent<NPCHealth>(out var npcHealth))
                return;
            
            var directionToTarget = hitColliders[0].transform.position - position;

            var angleToTarget = Vector3.Angle(-transform.forward, directionToTarget);

            // Check if the target is within the attraction distance and angle
            if (angleToTarget <= detectionAngle)
            {
                enemy = hitColliders[0].gameObject;

                switch (attacksAnything)
                {
                    case false when currentTimeUntilFlee <= 0:
                        StartCoroutine(FleeCoroutine());
                        break;
                    case true:
                        rb.velocity = Vector3.zero;
                        behaviour = Behaviour.Attack;
                        break;
                }
            }
        }
    }

    public IEnumerator StopMovement(float time)
    {
        var speedBefore = currentSpeed;
        
        stopMovement = true;
        currentSpeed = 0;
        yield return new WaitForSeconds(time);
        currentSpeed = speedBefore;
        stopMovement = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}