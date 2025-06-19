using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class Soul : MonoBehaviour
{
    public event Action<bool> OnLevitationEnd;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private SoulType soulType;
    private Vector2 basePosition;
    private Tween levitateTween;
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
        transform.position = basePosition = position;
        lifeTime = duration;
        spriteRenderer.DOFade(1, 0.15f);
        transform.DOMoveY(basePosition.y + 0.5f, 0.15f);
    }

    public Tween DespawnSoul()
    {
        return spriteRenderer.DOFade(0, 0.5f);
    }

    public void PurifySoul()
    {
        isPurified = true;
        animator.SetTrigger("IsPurified");
        spriteRenderer.DOColor(Color.blue, 0.2f)
                        .SetEase(Ease.InOutQuad)
                        .SetLoops(1, LoopType.Yoyo);
        Debug.Log("Purify has worked !!");
        StopLevitate();
    }

    private void Levitate()
    {
        Debug.Log("Start Levitate");
        float distance = 0.5f;
        float tweenDuration = 1f;
        if (levitateTween != null && levitateTween.IsActive()) return;

        levitateTween = transform.DOMoveY(transform.position.y + distance, tweenDuration)
                                 .SetEase(Ease.InOutQuad)
                                 .SetLoops(-1, LoopType.Yoyo);

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
        if (levitateTimer != null) StopCoroutine(levitateTimer);
        spriteRenderer.DOColor(Color.red, 0.2f).OnComplete(() =>
        {
            OnLevitationEnd?.Invoke(isPurified);
        });
    }

}

public enum SoulType
{
    Parasite,
    Lost,
    Evil
}
