using NaughtyAttributes;
using UnityEngine;

public class AnimationEffectTriggers : MonoBehaviour
{
    private Animator _animator;
    private AnimationEffect m_Effects;
    public AnimationEffect AnimationEffect { get { return m_Effects; } }


    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (m_Effects)
        {
            case AnimationEffect.None:
                break;
            case AnimationEffect.Attack:
                if (collision.TryGetComponent(out EnemyBehaviour enemy))
                {
                    Debug.Log("TRY DO DAMAGES");
                    enemy.TakeDamage(3.5f);
                }
                break;
            case AnimationEffect.Purification:
                if (collision.TryGetComponent(out Soul soul))
                {
                    Debug.Log("TRY PURIFY");
                    soul.PurifySoul(transform.position);
                }
                break;

        }
    }

    public void PlayAnim(AnimationEffect effect) 
    {
        Debug.Log("Play anim");
        m_Effects = effect;
        switch (m_Effects)
        {
            case AnimationEffect.None:
                break;
            case AnimationEffect.Attack:
                _animator.SetTrigger("SlashBaseAttack");
                break;
            case AnimationEffect.Purification:
                _animator.SetTrigger("Purification");
                break;
            case AnimationEffect.GameOver:
                _animator.SetTrigger("GameOver");
                break;
        }
    }

    //public void ActiveAnimationEffectAttack()
    //{
    //    m_Effects = AnimationEffect.Attack;
    //}

    public void ResetAnimationEffect()
    {
        m_Effects = AnimationEffect.None;
        ResetAllTriggers();
    }

    private void ResetAllTriggers()
    {
        foreach (var param in _animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                _animator.ResetTrigger(param.name);
            }
        }
    }
}

public enum AnimationEffect
{
    None,
    Attack,
    Purification,
    GameOver
}
