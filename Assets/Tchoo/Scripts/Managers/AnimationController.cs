using System;
using System.Collections;
using UnityEngine;

namespace Assets.Tchoo.Scripts.Managers
{
    public class AnimationController : MonoBehaviour
    {
        protected Animator animator;
        protected string currentState;
        protected string previousState;

        // Use this for initialization
        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        protected void ChangeAnimationState(string newState)
        {
            if(currentState == newState) return;
            animator.Play(newState);
            currentState = newState;
        }
    }
}

namespace AnimationState { 

    public enum Player{
        Idle,
        Walk,
        Run,
        Turn,
        StartJump,
        Jump,
        Apex,
        Fall,
        Land,
        Attack,
        Purify,
        GetHit,
        Death,
        AirAttack,
    }

    public enum Enemy
    {
        example1,
        example2, 
        example3
    }
}