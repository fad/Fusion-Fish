using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class Attack : MonoBehaviour
{
    [Header("Food")] 
    [SerializeField] private LayerMask foodLayerMask;
    private GameObject foodObject;

    [Header("Bite")] 
    [SerializeField] private Transform biteUpper;
    [SerializeField] private Transform biteLower;
    [SerializeField] private Animator biteAnimator;
    
    [Header("Attack")] 
    public bool isAttacking;
    private float attackDamage;
    private bool preparedAttack;
    
    [Header("Time")]
    private float timeBetweenAttack;
    [SerializeField] private float maxTimeBetweenAttack = 0.375f;

    [Header("Player")]
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private Transform fishRender;

    private void Start() => timeBetweenAttack = maxTimeBetweenAttack;

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
                preparedAttack = true;
                break;
            case false when preparedAttack:
            {
                biteAnimator.SetTrigger("executeAttack");
                thirdPersonController.animator.SetTrigger("attack");
                timeBetweenAttack = maxTimeBetweenAttack;
                if (foodObject != null)
                {
                    fishRender.transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(foodObject.transform.position - fishRender.transform.position), Time.deltaTime * 10);
                } 
                preparedAttack = false;
                break;
            }
        }
    }

    public void ResetBiteImage()
    {
        biteUpper.gameObject.SetActive(false);
        biteLower.gameObject.SetActive(false);
        biteUpper.GetComponent<Image>().color = Color.white;
        biteLower.GetComponent<Image>().color = Color.white;
    }

    private void DamageAnimationTrigger()
    {
        if (foodObject != null)
        {
            foodObject.GetComponent<Health>().ReceiveDamage(1);
            foodObject = null;
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
