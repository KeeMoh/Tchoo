using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHandler : AnimationController
{
    [SerializeField] private AnimationState[] animStateArray;
    private Dictionary<EPlayerAnim, AnimationState> playerAnimMap;
    private bool isGrounded;
    private Vector2 currentVelocity;

    private void Start()
    {
        //var ff = Resources.LoadAll<AnimationState>("Resources/Animations");
        base.Start();
        playerAnimMap = new();

        Debug.Log("count before ===> " + playerAnimMap.Count);
        foreach (AnimationState animState in animStateArray)
        {
            playerAnimMap[animState.EPlayerAnim] = animState;
            //animator.GetAnimatorTransitionInfo();
            playerAnimMap[animState.EPlayerAnim].SetAnimationHash(Animator.StringToHash(animState.AnimationName));
        }
        Debug.Log("count ===> " + playerAnimMap.Count);
    }

    public void TryTriggerAnim(EPlayerAnim anim)
    {
        CallAnimation(anim);
    }

    public void HandleGroundedStateChange(bool grounded)
    {
        if (grounded)
        {
            Debug.Log("HandleGroundedStateChange From Player Animation > grounded true, call anim landing.");
            CallAnimation(EPlayerAnim.Landing);
        }
        else
        {
            if(currentVelocity.y < Mathf.Epsilon)
            {
            Debug.Log("HandleGroundedStateChange From Player Animation > grounded false, call apex");
                CallAnimation(EPlayerAnim.Apex);
            }
            else
            {
                Debug.Log("HandleGroundedStateChange From Player Animation > grounded false, call startJump");
                CallAnimation(EPlayerAnim.StartJump);
            }
        }
        isGrounded = grounded;
    }

    public void SetVelocity(Vector2 velocity)
    {
        currentVelocity = velocity;
        if (isGrounded)
        {
            if (velocity.magnitude > Mathf.Epsilon)
            {
                if (velocity.magnitude > 3)
                {
                    CallAnimation(EPlayerAnim.Run);
                }
                else
                {
                    CallAnimation(EPlayerAnim.Walk);
                }
            }
            else
            {
                CallAnimation(EPlayerAnim.Idle);
            }
        }
        else
        {
            if (velocity.y > Mathf.Epsilon)
            {
                CallAnimation(EPlayerAnim.Jump);
            }
            else if (velocity.y < -0.02f)
            {
                CallAnimation(EPlayerAnim.Fall);
            }
            else 
            { 
                CallAnimation(EPlayerAnim.Apex);
            }
        }

    }

    public bool IsCurrentAnimation(EPlayerAnim playerAnim)
    {
        return playerAnimMap[currentState.EPlayerAnim] == playerAnimMap[playerAnim];
    }

    public EPlayerAnim GetCurrentAnimation()
    {
        if (currentState == null || !playerAnimMap.ContainsKey(currentState.EPlayerAnim))
            return EPlayerAnim.None;
        return currentState.EPlayerAnim;
    }

    protected void CallAnimation(EPlayerAnim playerAnim)
    {
        //Debug.Log("Call Animation " + playerAnim.ToString());
        try
        {
            TryChangeAnimationState(playerAnimMap[playerAnim]);
        }
        catch(Exception e)
        {
            Debug.LogWarning($"Animation {playerAnim} not found in animStateArray, exception : {e}");
        }   
    }
}

[Serializable]
public enum EPlayerAnim
{
    None,
    Idle,
    Walk,
    Run,
    Turn,
    StartJump,
    Jump,
    Apex,
    Fall,
    Landing,
    Attack,
    Purify,
    GetHit,
    GameOver,
    AerialAttack,
}