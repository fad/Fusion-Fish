using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

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

    private void Start()
    {
        maxSensitivity = thirdPersonController.sensitivity;
        halfSensitivity = thirdPersonController.sensitivity /= 2;
        
        timeBetweenAttack = maxTimeBetweenAttack;
    }

    private void Update() => AttackUpdate();

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
        if (foodObject != null)
        {
            var experienceValueOfEnemy = foodObject.GetComponent<Health>().experienceValue;
            foodObject.GetComponent<Health>().ReceiveDamage(attackDamage);
            if (foodObject.GetComponent<Health>().currentHealth <= 0)
            {
                if (foodObject.GetComponent<NPC>())
                {
                    FindObjectOfType<NPCSpawner>().SpawnFish();
                }
                else
                {
                    FindObjectOfType<FoodSpawner>().SpawnFood();
                }
                thirdPersonController.playerManager.experience.currentExperience += experienceValueOfEnemy;
                foodObject.SetActive(false);
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
