using Unity.VisualScripting;
using UnityEngine;

public abstract class AnimationController : MonoBehaviour
{
    protected Animator animator;
    protected AnimationState currentState;
    //protected AnimationState previousState;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected void TryChangeAnimationState(AnimationState newState)
    {
        
        if (currentState == newState)
        {
            return;
        }

        if (!IsValidTransition(newState))
        {
            Debug.LogWarning("The new state animation is not a valid transition.");
            return;
        }
        Debug.Log("animation " + currentState?.AnimationName +" switch to " + newState.AnimationName);
        animator.Play(newState.Id);
        currentState = newState;
    }

    protected bool IsValidTransition(AnimationState requestedAnimation) 
    {
        if (currentState == null) return true;
        //If higher priority
        if (requestedAnimation.PriorityLevel < currentState?.PriorityLevel) return true;

        //If current anim has not reaches the cancellable point.
        if (GetCurrentAnimationPercentage() < currentState?.CancelableAtPercent) {
            Debug.Log("Not valid transition " + currentState.AnimationName + " to " + requestedAnimation.AnimationName + ": " + GetCurrentAnimationPercentage() + "/" + currentState.CancelableAtPercent + " _ loop : " + currentState.Loop);
            return false; 
        }
        Debug.Log("Valid transition " + currentState.AnimationName + " to " + requestedAnimation.AnimationName + ": " + GetCurrentAnimationPercentage() + "/" + currentState.CancelableAtPercent + " _ loop : " + currentState.Loop);
        return true;
    }

    //protected float GetAnimationLength(AnimationState animation)
    //{
    //    AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
    //    foreach (AnimationClip clip in clips)
    //    {
    //        if (clip.name == animation.EPlayerAnim.DisplayName())
    //            return clip.length;
    //    }
    //    return -1f;
    //}

    protected float GetCurrentAnimationPercentage()
    {
        if (currentState.Loop)
        {
            return animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
        }
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
}