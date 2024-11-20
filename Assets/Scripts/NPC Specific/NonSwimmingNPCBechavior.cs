using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class NonSwimmingNPCBechavior : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float rotationSpeed = 1;
    [SerializeField] private float defaultSwimSpeed = 10f;
    private Quaternion lookDirection;
    private Vector3 direction;

    private Rigidbody rb;
    [Header("Movement")]

    [SerializeField] private Animator animator;
    [SerializeField] private HealthManager healthManager;

    private float checkObstacleDistance = 1;
    private bool OnHide;

    [SerializeField] private LayerMask groundLayer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(UpdateLookDirection());
        healthManager.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        healthManager.OnHealthChanged -= OnHealthChanged;
    }

    public void OnHealthChanged(float health)
    {
        if (health != healthManager.maxHealth)
        {
            OnHide = true;
            animator.SetBool("hide", true);
            animator.SetBool("walk", false);
        }
        else 
        { 
            OnHide = false;
            animator.SetBool("hide", false);
        }
    }
    IEnumerator UpdateLookDirection()
    {
        while (true)
        {
            NewDirection();

            if (!CheckObstacle())
                yield return new WaitForSeconds(10);
            else
                yield return null;
        }
    }

    private void NewDirection()
    {
        direction = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
        lookDirection = Quaternion.LookRotation(direction);
        direction *= 10;
    }

    private void FixedUpdate()
    {
        if (OnHide)
            return;

        MoveNPCInDirection();
        RotateToDirection();
    }
    private void RotateToDirection()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, lookDirection, Time.deltaTime * rotationSpeed);
    }
    private void MoveNPCInDirection()
    {
        transform.Translate(Vector3.forward * defaultSwimSpeed * Time.deltaTime);
        animator.SetBool("walk", true);
    }

    private bool CheckObstacle()
    {
        return Physics.Raycast(transform.position, direction.normalized, checkObstacleDistance, groundLayer);
    }
}
