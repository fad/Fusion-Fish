using System;
using System.Collections;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPC : NetworkBehaviour
{
    [Header("Movement")] 
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxSwimSpeed = 95f;
    [SerializeField] private float defaultSwimSpeed = 30f;
    [SerializeField] private bool mainMenuObject;
    private float currentSpeed;
    private bool slowDownNPC;

    [Header("Layers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask foodLayer;
    [SerializeField] private LayerMask stateAuthorityPlayer;
    [SerializeField] private LayerMask groundLayer;
    private GameObject enemy;

    [Header("Animation")]
    private Animator animator;

    [Header("Attack")] 
    [SerializeField] private float timeBetweenAttacksMax = 1.5f;
    [SerializeField] private float timeBetweenAttacksMin = .8f;
    [SerializeField] private bool attacksPlayer;
    [SerializeField] private int attackDamage = 10;
    private float enemyRange;
    private bool isAttacking;
    [SerializeField] private float attackRange;
    [SerializeField] private float attractionAngle;

    [Header("Flight")]
    [SerializeField] private float maxFlightDelayTime;
    private bool canFlight = true;
    private float currentFlightDelayTime;
    private const float FlightTime = 3f;
    
    [Header("Natural Behaviour")]
    [SerializeField] private float minWaitTime;
    [SerializeField] private float maxWaitTime;
    [SerializeField] private float minTimeChangeMoveDirection = 6f;
    [SerializeField] private float maxTimeChangeMoveDirection = 10f;
    private Rigidbody rb;
    private Vector3 newPosition;
    [Tooltip("time when the npc changes direction of movement")] 
    private float randomTime;
    private float currentWaitTime;

    private Behaviour behaviour;
    private enum Behaviour
    {
        NaturalBehaviour,
        Flight,
        Attack,
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        behaviour = Behaviour.NaturalBehaviour;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        DoBehaviour();
        
        CheckForEnemies();

        if (!attacksPlayer)
        {
            //Keeps up with when the NPC is able to flee again
            if (currentFlightDelayTime > 0)
            {
                canFlight = false;
                currentFlightDelayTime -= Time.deltaTime;
            }
            else
            {
                canFlight = true;
            }   
        }
    }

    //flees in opposite direction of the player, otherwise goes back to the natural movement of the NPC
    //But if the NPC gets attacked it cannot flee nor go back in the usual movement
    private void DoBehaviour()
    {
        switch (behaviour)
        {
            case Behaviour.Flight:
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
                var currentPosition = transform.position;

                if (randomTime <= 0)
                {
                    newPosition = new Vector3(RandomFloat(currentPosition.x), RandomFloat(currentPosition.y), RandomFloat(currentPosition.z));

                    randomTime = Random.Range(minTimeChangeMoveDirection, maxTimeChangeMoveDirection);
                    currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
                }

                if (currentWaitTime > 0)
                {
                    currentWaitTime -= Time.deltaTime;
                }
                
                if (currentWaitTime <= 0)
                {
                    randomTime -= Time.deltaTime;
                    
                    currentSpeed = defaultSwimSpeed;

                    //Make NPC not swim toward objects
                    if (Physics.Raycast(currentPosition, -transform.forward, out var hit, 1, groundLayer))
                    {
                        newPosition = new Vector3(hit.normal.x, hit.normal.y, hit.normal.z);
                        newPosition *= 5;
                    }

                    MoveNPCInDirection(newPosition - currentPosition, Quaternion.LookRotation(newPosition - currentPosition));
                }
                break;
            case Behaviour.Attack:
                if (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) < 15 && !enemy.GetComponent<Health>().isDead && !Physics.Raycast(transform.position, -transform.forward, 1, groundLayer))
                {
                    if (Vector3.Distance(transform.position, enemy.transform.position) < 1)
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

        if(!mainMenuObject)
            animator.SetFloat("movingSpeed", rb.velocity.sqrMagnitude * 3);
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        
        yield return new WaitForSeconds(Random.Range(timeBetweenAttacksMin, timeBetweenAttacksMax));
        
        animator.SetTrigger("attack");
        
        yield return new WaitForSeconds(.2f);
        
        if(enemy != null && Vector3.Distance(transform.position, enemy.transform.position) < 1.5f && !enemy.GetComponent<Health>().isDead)
        {
            SubtractHealth();
        }

        isAttacking = false;
    }

    private void SubtractHealth()
    {
        enemy.GetComponent<Health>().ReceiveDamageRpc(attackDamage, true);
    }
    
    private void MoveNPCInDirection(Vector3 targetDirection, Quaternion lookDirection)
    {
        if (TryGetComponent<Health>(out var health) && health.slowNPCDown)
        {
            rb.AddForce(targetDirection.normalized * (-currentSpeed / 2 * Time.deltaTime), ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(targetDirection.normalized * (-currentSpeed * Time.deltaTime), ForceMode.Impulse);
        }
        
        transform.rotation = Quaternion.Lerp(transform.rotation, lookDirection, Time.deltaTime * rotationSpeed);
    }
    
    private float RandomFloat(float value)
    {
        return Random.Range(value - 4.5f, value + 4.5f);
    }

    //Flees for a certain amount of time and goes back into normal movement behaviour
    private IEnumerator FlightCoroutine()
    {
        behaviour = Behaviour.Flight;

        yield return new WaitForSeconds(FlightTime);

        rb.velocity = Vector3.zero;
        currentFlightDelayTime = maxFlightDelayTime;
        behaviour = Behaviour.NaturalBehaviour;
    }

    //Checks if the player entered the vision of the NPC and flees when possible
    private void CheckForEnemies()
    {
        var hitColliders = new Collider[1];

        var position = transform.position;
        var foodHits = Physics.OverlapSphereNonAlloc(position, attackRange, hitColliders, foodLayer);
        var playerHits = Physics.OverlapSphereNonAlloc(position, attackRange, hitColliders, playerLayer);
        var playerStateAuthorityHits = Physics.OverlapSphereNonAlloc(position, attackRange, hitColliders, stateAuthorityPlayer);
        
        //Checks for two players because the player always detects itself first
        if ((foodHits >= 1 && transform.localScale.z > hitColliders[0].transform.localScale.z) || playerHits >= 1 || playerStateAuthorityHits >= 1 && !hitColliders[0].GetComponent<Health>().notAbleToGetBitten  && hitColliders[0].GetComponent<Health>() != GetComponent<Health>())
        {
            var directionToTarget = hitColliders[0].transform.position - transform.position;

            var angleToTarget = Vector3.Angle(-transform.forward, directionToTarget);

            // Check if the target is within the attraction distance and angle
            if (angleToTarget <= attractionAngle)
            {
                enemy = hitColliders[0].gameObject;
        
                if (!attacksPlayer && canFlight)
                {
                    StartCoroutine(FlightCoroutine());
                }
        
                if(attacksPlayer)
                {
                    rb.velocity = Vector3.zero;
                    behaviour = Behaviour.Attack;
                }
            }
        }
    }
}