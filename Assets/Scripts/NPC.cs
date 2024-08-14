using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPC : MonoBehaviour
{
    [Header("Movement")] 
    private float currentSpeed;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxSwimSpeed = 95f;
    [SerializeField] private float maxSwimSpeedOnAttack = 15f;
    [SerializeField] private float defaultSwimSpeed = 30f;
    private Rigidbody rb;

    [Header("things in NPCs view")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask foodLayer;
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

    [Header("Flight")]
    [SerializeField] private float maxFlightDelayTime;
    private bool canFlight = true;
    private float currentFlightDelayTime;
    private const float FlightTime = 3f;
    [Tooltip("time when the npc changes direction of movement")]
    private float randomTime;

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
                var swimDirectionAwayFromEnemy = enemy.transform.position - transform.position;
                
                currentSpeed = GetComponent<Health>().currentHealth >= GetComponent<Health>().maxHealth ? maxSwimSpeedOnAttack : maxSwimSpeed;
                
                MoveNPCInDirection(swimDirectionAwayFromEnemy, Quaternion.LookRotation(swimDirectionAwayFromEnemy));
                break;
            case Behaviour.NaturalBehaviour:
                var currentPosition = transform.position;
                var newPosition = currentPosition;

                if (randomTime <= 0)
                {
                    newPosition = new Vector3(RandomFloat(currentPosition.x), RandomFloat(currentPosition.y), RandomFloat(currentPosition.z));
                    randomTime = Random.Range(4.5f, 7.5f);
                    defaultSwimSpeed = Random.Range(3, 5);
                }
                    
                randomTime -= Time.deltaTime;

                currentSpeed = defaultSwimSpeed;
                    
                MoveNPCInDirection(newPosition - currentPosition, Quaternion.LookRotation(newPosition - currentPosition));
                break;
            case Behaviour.Attack:
                if (Vector3.Distance(transform.position, enemy.transform.position) < 1)
                {
                    if (!isAttacking)
                    {
                        StartCoroutine(AttackCoroutine());
                    }
                }
                else if (Vector3.Distance(transform.position, enemy.transform.position) > 15)
                {
                    behaviour = Behaviour.NaturalBehaviour;
                }
                else
                {
                    var swimDirectionEnemy = transform.position - enemy.transform.position;

                    currentSpeed = maxSwimSpeed;
                
                    MoveNPCInDirection(swimDirectionEnemy, Quaternion.LookRotation(swimDirectionEnemy));   
                }
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        animator.SetFloat("movingSpeed", rb.velocity.sqrMagnitude);
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        
        yield return new WaitForSeconds(Random.Range(timeBetweenAttacksMin, timeBetweenAttacksMax));
        
        animator.SetTrigger("attack");
        
        yield return new WaitForSeconds(.2f);
        
        SubtractHealth();

        isAttacking = false;
    }

    private void SubtractHealth()
    {
        enemy.GetComponent<Health>().ReceiveDamage(attackDamage);
    }
    
    private void MoveNPCInDirection(Vector3 targetDirection, Quaternion lookDirection)
    {
        rb.AddForce(targetDirection * (-currentSpeed * Time.deltaTime), ForceMode.Impulse);
        
        if (rb.velocity.sqrMagnitude > .1f) 
        {  
            transform.rotation = Quaternion.Lerp(transform.rotation, lookDirection, Time.deltaTime * rotationSpeed);
        }
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
    private void OnTriggerEnter(Collider col)
    {
        if ((1 << col.gameObject.layer) == playerLayer.value || (1 << col.gameObject.layer) == foodLayer.value)
        {
            enemy = col.GetComponent<Transform>().transform.gameObject;
            
            if (!attacksPlayer)
            {
                if (canFlight)
                {
                    StartCoroutine(FlightCoroutine());
                }
            }
            
            if(attacksPlayer)
            {
                rb.velocity = Vector3.zero;
                behaviour = Behaviour.Attack;
            }
        }
    }
}