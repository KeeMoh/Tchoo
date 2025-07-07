using Assets.Tchoo.Scripts.Managers;
using UnityEngine;

public class PlayerAnimationHandler : AnimationController
{
    public void SetVelocity(Vector2 velocity)
    {
        if(velocity.magnitude > Mathf.Epsilon)
        {
            if(velocity.magnitude > 3) 
            { 
                ChangeAnimationState("Run");
            }
            else
            {
                ChangeAnimationState("Walk");
            }
        }
        else
        {
            ChangeAnimationState("Idle");
        }

        if(velocity.y < Mathf.Epsilon)
        {
            ChangeAnimationState("Apex");
        }
        //animator.SetFloat("yVelocity", velocity.y);
        //animator.SetFloat("magnitude", velocity.magnitude);
    }
}
