using System;
using UnityEngine;

public class PlayerMovementHandler : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [Header("Move")]
    [SerializeField] private float moveSpeed;
    [SerializeField, Range(0.05f, 2f)] private float turnSpeed = 0.25f;
    private float defaultMoveSpeed;
    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float[] jumpHoldDurations;
    [SerializeField] private float[] minimunJumpForceForDurations;
    [SerializeField, Range(0, 1)] private float decelerationValue;
    private float defaultJumpForce;
    [Header("Double Jump")]
    [SerializeField] private float doubleJumpForce;
    [SerializeField] private int maxJump;
    private int jumpRemaining;
    [Header("Falling")]
    [SerializeField] private float fallMinSpeed;
    [SerializeField] private float fallBaseSpeed;
    [SerializeField] private float fallMaxSpeed;
    [Header("Gravity")]
    [SerializeField] private float baseGravity;
    [SerializeField] private float gravityMultiplier;
    private float defaultGravity;
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

    private bool wasGrounded = false;
    private float timeSinceGrounded = 0f;
    private float timeSinceJumpPressed = 0f;
    private bool hasJustPressedJump = false;
    private float timeAllowedForCoyoteJump = 0.22f;
    private float timeAllowedForPreJump = 0.12f;
    bool isWallJumping;
    public bool IsWallJumping => isWallJumping;
    private float jumpPressedTime;
    private float jumpPressedTimeDelta;
    private bool isHoldingJump;
    private bool isInFirstJumpAscent = false;
    private bool endFirstJump = false;
    private bool wallJumpIsActive = false;
    private float horizontalMovement;
    private float verticalMovement;

    //public event Action OnLanded;
    public event Action OnFlipRequested;
    public event Action<bool> OnGroundedStateChanged;
    public event Action<JumpType> OnJumpProcessed;
    public Vector2 CurrentVelocity => rb.linearVelocity;

    private void Start()
    {
        defaultJumpForce = jumpForce;
        defaultMoveSpeed = moveSpeed;
        defaultGravity = baseGravity;
    }

    //Debug
    public void SwitchSettings(JumpSettings settings)
    {
        jumpForce = settings.JumpForce;
        jumpHoldDurations = settings.JumpHoldDurations;
        minimunJumpForceForDurations = settings.MinimumJumpForceForDurations;
        gravityMultiplier = settings.GravityMultiplier;
        decelerationValue = settings.DecelerationValue;
    }

    //Called every frame from playerController
    public void HandleMovementInput(float horizontalAxis, float verticalAxis, bool jumpInput)
    {
        horizontalMovement = horizontalAxis;
        verticalMovement = verticalAxis;
        isHoldingJump = jumpInput;
        CheckIsGrounded();
        if (wasGrounded) jumpRemaining = maxJump;
        if (hasJustPressedJump) TryExecutePreJump();
        if (wallJumpIsActive) processWallJump();
    }

    public void ApplyGroundMovement()
    {
        rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, moveSpeed * horizontalMovement, turnSpeed);
    }

    private void ApplyAerialMovement()
    {
        rb.linearVelocityY = isWallSliding()
            ? Mathf.Clamp(rb.linearVelocityY, wallSlideSpeed * -1f, jumpForce)
            : Mathf.Clamp(rb.linearVelocityY, GetFallSpeed(), jumpForce);
        //CheckFallingSpeed(); // --> this function set the camera Y offset.
    }

    private float GetFallSpeed()
    {
        if (verticalMovement > 0.4f)
        {
            return fallMinSpeed;
        }
        if (verticalMovement < -0.4f)
        {
            return fallMaxSpeed;
        }
        return fallBaseSpeed;
    }

    public bool IsFallingFast() => rb.linearVelocityY < fallMinSpeed;

    public void ApplyMovement()
    {

        if (!isWallJumping)
        {
            ApplyGroundMovement();
            ApplyAerialMovement();
        }
        if (isInFirstJumpAscent)
        {
            HandleFirstJump();
        }

        if (endFirstJump)
        {
            HandleEndJump();
        }
        ProcessGravity();
    }

    public void StopMovement(int stop)
    {
        Debug.Log("Stop movement " + stop);
        jumpForce = stop == 1 ? 0 : defaultJumpForce;
        moveSpeed = stop == 1 ? 0 : defaultMoveSpeed;
    }

    public void EjectPlayer(Vector2 force) 
    { 
        rb.linearVelocity = force;
    }

    public void StopGravity(bool stop)
    {
        if (stop)
        {
            baseGravity = 0;
        }
        else
        {
            baseGravity = defaultGravity;
        }
    }

    private void TryExecutePreJump()
    {
        timeSinceJumpPressed += Time.deltaTime;
        //Debug.Log("ProcessJump since : " + timeSinceJumpPressed.ToString());
        if (timeSinceJumpPressed <= timeAllowedForPreJump)
        {
            if (wasGrounded && rb.linearVelocityY > -0.01f)
            {
                TryProcessJump();
                hasJustPressedJump = false;
                timeSinceJumpPressed = 0f;
            }
        }
        else
        {
            hasJustPressedJump = false;
            timeSinceJumpPressed = 0f;
        }
        
    }

    /// Increment timer while the jump button is pressed to define which step of #jumpHoldDurations the character reaches
    private void HandleFirstJump()
    {
        //Clamp minimum velocity based on minimunJumpForceForDurations values
        for (int i = 0; i < jumpHoldDurations.Length; i++)
        {
            if (jumpPressedTime + jumpPressedTimeDelta < jumpHoldDurations[i])
            {
                if (rb.linearVelocityY > Mathf.Epsilon && minimunJumpForceForDurations.Length > i)
                {
                    rb.linearVelocityY = Mathf.Max(rb.linearVelocityY, minimunJumpForceForDurations[i]);
                }

                //Debug.Log("VelocityY => " + rb.linearVelocityY +
                //    " | pressedTime => " + jumpPressedTime +
                //    " | Delta => " + jumpPressedTimeDelta +
                //    " | Jump n°" + (i + 1).ToString()
                //    );

                break;
            }
        }
        //Debug.Log("isHoldingJump ? " + isHoldingJump);
        if (isHoldingJump)
        {
            jumpPressedTime += Time.fixedDeltaTime;
            //If holding time reach the maximum
            if (jumpPressedTime >= jumpHoldDurations[jumpHoldDurations.Length - 1])
            {
                //Force Jump button released
                isHoldingJump = false;
                jumpPressedTimeDelta = 0;
                isInFirstJumpAscent = false;
                //debugText.text = jumpHoldDurations.Length.ToString();
                endFirstJump = true;
            }
        }
        else
        {
            HoldUntilNextStepOfJump();
        }
    }

    ///Simulate holding the jump button until next level of Jump Height timer
    private void HoldUntilNextStepOfJump()
    {
        //Debug.Log("HoldUntilNextStep");
        for (int i = 0; i < jumpHoldDurations.Length; i++)
        {
            //Find the next jump height level
            if (jumpPressedTime < jumpHoldDurations[i])
            {
                //debugText.text = (i + 1).ToString();
                jumpPressedTimeDelta += Time.fixedDeltaTime;
                if (jumpPressedTime + jumpPressedTimeDelta >= jumpHoldDurations[i])
                {
                    isInFirstJumpAscent = false;
                    endFirstJump = true;
                }
                break;
            }
        }
    }

    ///Decelerate if the character is still ascending
    private void HandleEndJump()
    {
        if (rb.linearVelocityY > Mathf.Epsilon)
        {
            rb.linearVelocityY = Mathf.Min(Mathf.Lerp(rb.linearVelocityY, 0f, decelerationValue), jumpForce / 2f);
            //Debug.Log("deceleration velocity : " + rb.linearVelocityY.ToString());
        }
        else
        {
            //Debug.Log("end deceleration velocity : " + rb.linearVelocityY.ToString());
            if (rb.linearVelocityY > -0.5f) rb.linearVelocityY = 0;
            //Debug.Log("end deceleration velocity : " + rb.linearVelocityY.ToString());
            endFirstJump = false;
        }
    }

    public void IncreaseMaxJump()
    {
        maxJump++;
    }

    public void ResetMaxJump()
    {
        maxJump = 1;
    }

    public void UnlockWallJump(bool unlock)
    {
        wallJumpIsActive = unlock;
    }

    private void ProcessGravity()
    {
        //Lower gravity when end jumping;
        if (endFirstJump)
        {
            rb.gravityScale = baseGravity / gravityMultiplier;
            return;
        }
        //Higher gravity when falling
        if (rb.linearVelocityY < -0.75f)
        {
            rb.gravityScale = baseGravity * gravityMultiplier;
            return;
        }
        //Otherwise, set as default value
        rb.gravityScale = baseGravity;
    }

    private bool isWallSliding()
    {
        if (!wasGrounded && IsBesideWall() && horizontalMovement != 0)
        {
            return true;
        }
        return false;
    }

    private bool IsBesideWall()
    {
        if (Physics2D.OverlapBox(wallCheckPos.position + (Vector3)wallCheckOffset, wallCheckSize, 0, wallLayer))
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
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void cancelWallJump()
    {
        isWallJumping = false;
    }

    //Called by inputs or hasJustPressedJump (TryPreJump)
    public void TryProcessJump()
    {
        Debug.Log("PROCESS JUMP");

        if (isWallSliding() && wallJumpTimer > 0f && wallJumpIsActive)
        {
            Debug.Log("WallJump");
            isWallJumping = true;

            //reset first jump logic
            isInFirstJumpAscent = false;
            endFirstJump = false;

            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            OnJumpProcessed?.Invoke(JumpType.WallJump);
            //animator.SetTrigger("jump"); TODO anim
            //wallJumpEffect.Play(); TODO vfx

            wallJumpTimer = 0f;

            //forceFlip
            if (transform.localScale.x != wallJumpDirection)
            {
                OnFlipRequested?.Invoke();
                //Flip();
            }

            //Regain double jumps
            jumpRemaining = maxJump - 1;

            Invoke(nameof(cancelWallJump), wallJumpTime);
            return;
        }
        if (wasGrounded || (timeSinceGrounded < timeAllowedForCoyoteJump && jumpRemaining == maxJump))
        {
            Debug.Log("-------- start Jump --------");
            rb.linearVelocityY = jumpForce;
            //isHoldingJump = true;
            OnJumpProcessed?.Invoke(JumpType.BaseJump);
            StopAllCoroutines();
            jumpPressedTime = 0;
            jumpPressedTimeDelta = 0;
            isInFirstJumpAscent = true;
            jumpRemaining--;
            return;
        }
        if (jumpRemaining > 0)
        {
            if (jumpRemaining == maxJump) jumpRemaining--;
            if (jumpRemaining != 0)
            {
                //reset first jump logic
                isInFirstJumpAscent = false;
                endFirstJump = false;

                OnJumpProcessed?.Invoke(JumpType.DoubleJump);
                rb.linearVelocityY = doubleJumpForce;
                jumpRemaining--;
                return;
            }
        }
        Debug.Log("Cant jump right now, try to process soon...");
        hasJustPressedJump = true;
    }

    //Called by inputs
    public void ReleaseJumpButton()
    {
        isHoldingJump = false;
    }

    private void CheckIsGrounded()
    {
        bool currentlyGrounded = Physics2D.OverlapBox(
            groundCheckPos.position + (Vector3)groundCheckOffset,
            groundCheckSize,
            0f,
            groundLayer);

        if (currentlyGrounded)
        {
            if (Mathf.Abs(rb.linearVelocityY) > Mathf.Epsilon)
            {
                if (wasGrounded) { OnGroundedStateChanged?.Invoke(false); }
                
                wasGrounded = false;
                return; // still in the air / have freshly jumped
            }

            if (!wasGrounded && timeSinceGrounded > 0.1f) // freshly landed
            {
                OnGroundedStateChanged?.Invoke(true);
                //OnLanded?.Invoke();
            }

            timeSinceGrounded = 0f;
            wasGrounded = true;
        }
        else
        {
            if (wasGrounded) { OnGroundedStateChanged?.Invoke(false); }
            timeSinceGrounded += Time.deltaTime;
            wasGrounded = false;
        }
    }

    public void FlipCheckOffsets(float direction)
    {
        wallCheckOffset.x = Mathf.Abs(wallCheckOffset.x) * Mathf.Sign(direction);
        groundCheckOffset.x = Mathf.Abs(groundCheckOffset.x) * Mathf.Sign(direction);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(groundCheckPos.position + (Vector3)groundCheckOffset, groundCheckSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(wallCheckPos.position + (Vector3)wallCheckOffset, wallCheckSize);
    }
}

public enum JumpType
{
    None,
    BaseJump,
    WallJump,
    DoubleJump
}