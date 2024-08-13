using System;
using System.Collections;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class Attack : MonoBehaviour
{
    [Header("Attack")] 
    [SerializeField] private LayerMask foodLayerMask;
    private float timeBetweenAttack;
    [SerializeField] private float maxTimeBetweenAttack = 0.375f;
    public bool isAttacking;
    private GameObject foodObject;
    private float attackDamage;
    public Animator animator;
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private Transform biteUpper;
    [SerializeField] private Transform biteLower;
    [SerializeField] private Animator biteAnimator;
    private bool preparedAttack;

    private void Start()
    {
        timeBetweenAttack = maxTimeBetweenAttack;
    }

    private void Update() => AttackUpdate();

    private void AttackUpdate()
    {
        if (timeBetweenAttack >= 0)
        {
            timeBetweenAttack -= Time.deltaTime;
            return;
        }
        
        if (thirdPersonController.input.attack && !preparedAttack)
        {
            biteUpper.gameObject.SetActive(true);
            biteLower.gameObject.SetActive(true);
            biteAnimator.SetTrigger("prepareAttack");
            preparedAttack = true;
        }
        
        if (!thirdPersonController.input.attack && preparedAttack)
        {
            biteAnimator.SetTrigger("executeAttack");
            animator.SetTrigger("attack");
            timeBetweenAttack = maxTimeBetweenAttack;
            preparedAttack = false;
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
