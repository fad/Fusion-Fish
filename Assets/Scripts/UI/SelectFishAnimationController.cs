using System.Collections;
using System.Collections.Generic;
using AvocadoShark;
using UnityEngine;
using CharacterType = AvocadoShark.Menu.CharacterType;

public class SelectFishAnimationController : MonoBehaviour
{
    [SerializeField] private Menu menu;
    [SerializeField] private CharacterType fishType;
    [SerializeField] private float maxScale;
    [SerializeField] private float speedBoostMoving = 6;
    private float scaleInterval = 0.01f;
    private float speedAnimation = 0.01f;

    private float normalScale;
    private Animator anim;
    void Start()
    {
        normalScale = transform.localScale.x;
        menu.newCharacterEvent += StartAnimation;
        anim = GetComponent<Animator>();
    }

    private void StartAnimation(CharacterType CharacterNumber)
    {
        StopAllCoroutines();
        anim.SetFloat("movingSpeed", 1);

        if (fishType == CharacterNumber)
            StartCoroutine(maxScaleFishAnimation());
        else
            StartCoroutine(normalScaleFishAnimation());
    }

    private IEnumerator maxScaleFishAnimation()
    {
        while (true)
        {
            if (transform.localScale.x < maxScale)
            {
                anim.SetFloat("movingSpeed", speedBoostMoving);
                transform.localScale += new Vector3(scaleInterval, scaleInterval, scaleInterval);
                yield return new WaitForSeconds(speedAnimation);
            }
            else
            {
                anim.SetFloat("movingSpeed", 1);
                StopAllCoroutines();
                break;
            }
        }

    }

    private IEnumerator normalScaleFishAnimation()
    {
        while (true)
        {
            if (transform.localScale.x > normalScale)
            {
                transform.localScale -= new Vector3(scaleInterval, scaleInterval, scaleInterval);
                yield return new WaitForSeconds(speedAnimation);
            }
            else
            {
                StopAllCoroutines();
                break;
            }
        }
    }
}