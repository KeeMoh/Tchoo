using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [Header("Move")]
    [SerializeField] private float moveSpeed;
    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float fallMinSpeed;
    [SerializeField] private float fallBaseSpeed;
    [SerializeField] private float fallMaxSpeed;
    [SerializeField] private float baseGravity;
    [SerializeField] private float gravityMultiplier;
    [Header("Double Jump")]
    [SerializeField] private float doubleJumpForce;
    [SerializeField] private int maxJump;
    private int jumpRemaining;
    [Header("GroundCheckSize")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private Vector2 groundCheckSize;
    [SerializeField] private LayerMask groundLayer;

    public float horizontalMovement;
    public float verticalMovement;

    void Update()
    {
        rb.linearVelocityX = moveSpeed * horizontalMovement;
        rb.linearVelocityY = Mathf.Clamp(rb.linearVelocityY, GetFallSpeed(), jumpForce);
        if (rb.linearVelocityY < -0.75f)
        {
            rb.gravityScale = baseGravity * gravityMultiplier;
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
        if(horizontalMovement > 0.4f)
        {
            horizontalMovement = 1;
        }
        if(horizontalMovement < -0.4f)
        {
            horizontalMovement = -1;
        }


        verticalMovement = context.ReadValue<Vector2>().y;
        if(verticalMovement > 0.4f)
        {
            verticalMovement = 1;
        }
        if(verticalMovement < -0.4f)
        {
            verticalMovement = -1;
        }
    }    
    
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && jumpRemaining > 0)
        {
            if(jumpRemaining == maxJump)
            {
                rb.linearVelocityY = jumpForce;
            }
            else
            {
                rb.linearVelocityY = doubleJumpForce;
            }
            jumpRemaining--;
        }
        if (context.canceled && rb.linearVelocityY > 0)
        {
            rb.linearVelocityY = 0;
        }
    }

    public void TellIsGrounded()
    {
        jumpRemaining = maxJump;
    }

    //private bool IsGrounded()
    //{
    //    if(Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
    //    {
    //        return true;
    //    }
    //    return false;
    //}

    private float GetFallSpeed()
    {
        if(verticalMovement > 0.4f)
        {
            return fallMinSpeed;
        }
        if (verticalMovement < -0.4f)
        {
            return fallMaxSpeed;
        }
        return fallBaseSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(groundCheckPos.position, groundCheckSize);
    }
}
