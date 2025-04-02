using System.Collections;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [Header("Move")]
    [SerializeField] private float moveSpeed;
    [Header("Jump")]
    [Description("time Max pressed for the jumpForce corresponding")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float[] timeToPressJump;
    [Header("Double Jump")]
    [SerializeField] private float doubleJumpForce;
    [SerializeField] private int maxJump;
    [Header("Falling")]
    [SerializeField] private float fallMinSpeed;
    [SerializeField] private float fallBaseSpeed;
    [SerializeField] private float fallMaxSpeed;
    [Header("Gravity")]
    [SerializeField] private float baseGravity;
    [SerializeField] private float gravityMultiplier;
    private int jumpRemaining;
    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private Vector2 groundCheckSize;
    [SerializeField] private Vector2 groundCheckOffset;
    [SerializeField] private LayerMask groundLayer;    
    [Header("WallCheck")]
    [SerializeField] private Transform wallCheckPos;
    [SerializeField] private Vector2 wallCheckSize;
    [SerializeField] private Vector2 wallCheckOffset;
    [SerializeField] private LayerMask wallLayer;
    [Header("WallMovement")]
    [SerializeField] private float wallSlideSpeed;    
    [Header("WallJump")]
    [SerializeField] private Vector2 wallJumpPower = new(5f, 10f);
    [SerializeField] private float wallJumpDirection;
    [SerializeField] private float wallJumpTime = 0.5f;
    [SerializeField] private float wallJumpTimer;
    [Header("Debug")]

    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshPro TMP;
    [SerializeField] private Image imgGround;
    [SerializeField] private Image imgWall;
    private Color debugColor = new(0f, 0f, 0f, 0f);

    bool isWallJumping;

    //[SerializeField] private bool isWallSliding;

    public float horizontalMovement;
    public float verticalMovement;
    private float jumpPressedTime;
    private bool isFirstJumpPressed;
    private bool isFacingRight = true;

    void Update()
    {
        if (IsGrounded()) jumpRemaining = maxJump;
        processWallJump();
        ProcessGravity();

        if(!isWallJumping)
        {
            UpdateMovement();
            Flip();
        }
        animator.SetFloat("yVelocity", rb.linearVelocityY);
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
    }

    private void UpdateMovement()
    {
        rb.linearVelocityX = moveSpeed * horizontalMovement;
        rb.linearVelocityY = isWallSliding() 
            ? Mathf.Clamp(rb.linearVelocityY, wallSlideSpeed*-1f, jumpForce) 
            : Mathf.Clamp(rb.linearVelocityY, GetFallSpeed(), jumpForce);
    }

    public void CollectFoolet()
    {
        maxJump++;
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
        if (horizontalMovement > 0.55f)
        {
            horizontalMovement = 1f;
        }
        else if(horizontalMovement > 0.05f)
        {
            horizontalMovement = 0.4f;
        }
        else if (horizontalMovement < -0.55f)
        {
            horizontalMovement = -1f;
        }
        else if (horizontalMovement < -0.05f)
        {
            horizontalMovement = -0.4f;
        }
        else
        {
            horizontalMovement = 0f;
        }


        verticalMovement = context.ReadValue<Vector2>().y;
        if(verticalMovement > 0.33f)
        {
            verticalMovement = 1;
        }
        if(verticalMovement < -0.33f)
        {
            verticalMovement = -1;
        }
    }
    
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && jumpRemaining > 0)
        {
            if(IsGrounded())
            {
                rb.linearVelocityY = jumpForce;
                isFirstJumpPressed = true;
                animator.SetTrigger("jump");
                StopAllCoroutines();
                StartCoroutine(handleFirstJumpTime());
            }
            else
            {
                if(jumpRemaining == maxJump) jumpRemaining--;
                if (jumpRemaining == 0) return;
                animator.SetTrigger("jump");
                rb.linearVelocityY = doubleJumpForce;
            }
            jumpRemaining--;
        }
        if (context.canceled && rb.linearVelocityY > 0)
        {
            isFirstJumpPressed = false; // Stop coroutine with this bool
        }

        //Wall Jump
        if(context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            animator.SetTrigger("jump");

            wallJumpTimer = 0f;

            //forceFlip
            if(transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
                wallCheckOffset.x *= -1f;
            }

            Invoke(nameof(cancelWallJump), wallJumpTime + 0.1f);
        }
    }

    private void ProcessGravity()
    {
        if (rb.linearVelocityY < -0.75f)
        {
            rb.gravityScale = baseGravity * gravityMultiplier;
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    private bool isWallSliding()
    {
        if(!IsGrounded() && IsBesideWall() && horizontalMovement != 0)
        {
            return true;
        }
        return false;
    }

    private void processWallJump()
    {
        if (isWallSliding())
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;
            
            CancelInvoke(nameof(cancelWallJump));
        }else if(wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void cancelWallJump()
    {
        isWallJumping = false;
    }

    private IEnumerator handleFirstJumpTime()
    {
        float jumpMaxPressTime = timeToPressJump[timeToPressJump.Length-1];
        jumpPressedTime = 0;
        //Debug.Log("start handleFirstJumpTime");
        while (isFirstJumpPressed)
        {
            jumpPressedTime += Time.deltaTime;
            if(!IsGrounded() && jumpRemaining == maxJump) { jumpRemaining--; }
            if(jumpPressedTime >= jumpMaxPressTime)
            {
                isFirstJumpPressed = false;
                TMP.text = (timeToPressJump.Length+1).ToString();
                //Debug.Log($"time max jump pressed ({jumpPressedTime})");
                yield return new WaitForSeconds(0.5f);
                TMP.text = "";
                yield break;
            }
            yield return null;
        }
        if(jumpPressedTime < jumpMaxPressTime)
        {
            for(int i = 0; i < timeToPressJump.Length; i++)
            {
                if (jumpPressedTime < timeToPressJump[i])
                {
                    //Debug.Log($"jump stop press at {jumpPressedTime} ({i+1}')");
                    TMP.text = (i+1).ToString();
                    yield return new WaitForSeconds(timeToPressJump[i] - jumpPressedTime);
                    //Debug.Log($"yield break after {timeToPressJump[i] - jumpPressedTime}");
                    StartCoroutine(stopPositiveVelocityY());
                    yield break;
                }
            }
            Debug.LogError($"/! JumpPressTime = {jumpPressedTime} & jumpPressTimeMax = {jumpMaxPressTime}, but the coroutine didnt do the condition to wait /!");
        }
        yield return null;
    }

    private IEnumerator stopPositiveVelocityY()
    {
        while (rb.linearVelocityY > 0.1f)
        {
            //Debug.Log("velocityY = " + rb.linearVelocityY);
            rb.linearVelocityY = Mathf.Min(Mathf.Lerp(rb.linearVelocityY, 0f, 0.15f),jumpForce/2);
            yield return null;
        }
        rb.linearVelocityY = 0;
        yield return new WaitForSeconds(0.3f);
        TMP.text = "";
    }

    private bool IsGrounded()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position + (Vector3)groundCheckOffset, groundCheckSize, 0, groundLayer))
        {
            //Debug.Log("detect ground");
            imgGround.color = Color.yellow;
            return true;
        }
        imgGround.color = debugColor;
        return false;
    }    
    
    private bool IsBesideWall()
    {
        if (Physics2D.OverlapBox(wallCheckPos.position + (Vector3)wallCheckOffset, wallCheckSize, 0, wallLayer))
        {
            //Debug.Log("detect wall");
            imgWall.color = Color.blue;
            return true;
        }
        imgWall.color = debugColor;
        return false;
    }

    private void Flip()
    {
        if(isFacingRight && horizontalMovement <0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
            wallCheckOffset.x *= -1f;
        }
    }

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
        Gizmos.DrawCube(groundCheckPos.position + (Vector3)groundCheckOffset, groundCheckSize);        
        
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(wallCheckPos.position + (Vector3)wallCheckOffset, wallCheckSize);
    }
}
