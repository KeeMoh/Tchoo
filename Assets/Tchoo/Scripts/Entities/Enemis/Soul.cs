using DG.Tweening;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Soul : MonoBehaviour
{
    public event Action<bool> OnLevitationEnd;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color baseColor;
    [SerializeField] private Color evilColor;
    [SerializeField] private Color endLevitationColor;
    [SerializeField] private Color purifiedColor;

    private SoulType soulType;
    private Vector2 basePosition;
    private Tween levitateTween;
    private Tween endLevitationTween;
    private Coroutine levitateTimer;
    private bool isPurified = false;
    private Animator animator;
    private float lifeTime = 0;

    private void Awake()
    {
        spriteRenderer.DOFade(0, 0);
        animator = GetComponent<Animator>();
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if(TryGetComponent(out AnimationEffectTriggers effect))
    //    {
    //        if(effect.AnimationEffect == AnimationEffect.Purification) 
    //        {
    //            PurifySoul();
    //        }
    //    }
    //}
    public void SpawnSoul(SoulType type, Vector2 position, float duration)
    {
        soulType = type;
        Debug.Log("SoulType : " + soulType + " (hash = " + soulType.GetHashCode() + ")");
        animator.SetInteger("SoulType", soulType.GetHashCode());
        transform.position = basePosition = position;
        lifeTime = duration;
        spriteRenderer.color = soulType == SoulType.Evil ? evilColor : baseColor;
        spriteRenderer.DOFade(1, 0.15f);

        transform.DOMoveY(basePosition.y + 0.5f, 0.15f);
        
    }

    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + transform.right * 2f, Color.green);
    }


    public void DespawnSoul()
    {
        if(soulType == SoulType.Evil)
        {
            if (!isPurified)
            {
                transform.rotation = Quaternion.identity;
            }
            else
            {
                animator.SetBool("IsPurified", isPurified);
                animator.SetTrigger("EndLife");
                return;
            }
        }

        Color color = isPurified ? purifiedColor : endLevitationColor;

        int loopCount = isPurified ? 1 : 6;

        endLevitationTween = spriteRenderer.DOColor(color, 0.4f)
                .SetEase(Ease.InOutQuad)
                .SetLoops(loopCount, LoopType.Yoyo).OnComplete(() =>
                {
                    animator.SetBool("IsPurified", isPurified);
                    animator.SetTrigger("EndLife");
                });
    }

    private void GoToSourceTween()
    {
        float distance = 20f;
        float tweenDuration = 0.5f;

        levitateTween = transform.DOMoveY(transform.position.y + distance, tweenDuration)
                             .SetEase(Ease.InOutQuad).OnComplete(() =>
                             {
                                 OnDespawnAnimationFinish();
                             });
    }

    //Called by animator
    private void OnDespawnAnimationFinish()
    {
        Debug.Log("InvoK OnDespawnAnimation (OnLevitationEnd)");
        OnLevitationEnd?.Invoke(isPurified);
    }

    public void PurifySoul(Vector3 fromLocation)
    {
        isPurified = true;
        if (soulType == SoulType.Evil)
        {
            levitateTween?.Kill();
            //Update Z rotation to face out "fromLocation"
            Vector3 direction = (transform.position - fromLocation).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle += 180f;
            Debug.Log("soul rotation angle => " + angle);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            //Debug.Log("soul purify fromLocation => " + fromLocation);
            //if (directionAway != Vector3.zero)
            //{
            //    Debug.Log("soul purify DirectionAway == " + directionAway.ToString());

            //    transform.rotation = Quaternion.LookRotation(directionAway);
            //}
            //else
            //{
            //    Debug.Log("soul purify DirectionAway == ZERO /!");
            //}
        }
        Debug.Log("Purify has worked !!");
        StopLevitate();
    }

    private void Levitate()
    {
        Debug.Log("Start Levitate");
        float distance = 0.35f;
        float tweenDuration = 1.5f;
        if (levitateTween != null && levitateTween.IsActive()) return;
        if (soulType == SoulType.Evil)
        {
            Debug.Log("Start soul Rotation");
            Vector3 rotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z + (30 * lifeTime)); // => 30°/sec
            Debug.Log("soul rotation goal : " + rotation);
            levitateTween = transform.DORotate(rotation, lifeTime, RotateMode.FastBeyond360);
        }
        else
        {
            Debug.Log("Start soul Levitation");
            levitateTween = transform.DOMoveY(transform.position.y + distance, tweenDuration)
                                 .SetEase(Ease.InOutQuad)
                                 .SetLoops(-1, LoopType.Yoyo);
        }
        levitateTimer = StartCoroutine(LevitateDurationTimer(lifeTime));
    }

    private IEnumerator LevitateDurationTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        if(isPurified) yield break;
        StopLevitate();
    }

    private void StopLevitate()
    {
        levitateTween?.Kill();
        endLevitationTween?.Kill();
        if (levitateTimer != null) StopCoroutine(levitateTimer);
        DespawnSoul();
    }

}

public enum SoulType
{
    Parasite,
    Lost,
    Evil
}
