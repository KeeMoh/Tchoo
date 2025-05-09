using DG.Tweening;
using NaughtyAttributes;
using System;
using System.Collections;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
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
    [Header("Corruption")]
    [SerializeField] private float minCorruption;
    [SerializeField] private float maxCorruption;
    [SerializeField] private Gradient colorCorruption;
    private float currentCorruption = 0f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem[] jumpEffects;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Light2D[] lights;
    [SerializeField] private ParticleSystem getFooletEffects;
    [SerializeField] private Material fooletMat;
    [SerializeField] private Animator animator;

    [Header("Debug")]
    [SerializeField] private TextMeshPro TMP;
    [SerializeField] private Image imgGround;
    [SerializeField] private Image imgWall;
    private Color debugColor = new(0f, 0f, 0f, 0f);

    bool isWallJumping;

    public event Action<float, bool> OnCorruptionValueChange;
    //[SerializeField] private bool isWallSliding;

    public float horizontalMovement;
    public float verticalMovement;
    private float jumpPressedTime;
    private bool isFirstJumpPressed;
    private bool isFacingRight = true;
    private float timeSinceGrounded = 0f;
    private float timeAllowedForJump = 0.22f;
    private bool isCorrupted = false;

    private void Start()
    {
        sprite.color = colorCorruption.Evaluate(0f);
        foreach(var item in lights)
        {
            item.color = colorCorruption.Evaluate(0f);
        }
        currentCorruption = minCorruption;
    }

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

    [Button]
    private void GainCorruption()
    {
        GainCorruption(0.5f);
    }    
    [Button]
    private void GainSanity()
    {
        GainSanity(0.5f);
    }
    public void GainCorruption(float corruptionValue)
    {
        bool switchState = false;
        if (currentCorruption == 0 && !isCorrupted)
        {
            isCorrupted = true;
            switchState = true;
        }
        
        currentCorruption = Mathf.Min(currentCorruption + corruptionValue, maxCorruption);

        if (!isCorrupted && currentCorruption > 0) currentCorruption = 0;
        //Changer pour valeur entre 0 et 1
        float range = maxCorruption - minCorruption;
        float delta = currentCorruption - minCorruption;
        float percentage = delta / range;
        sprite.DOColor(colorCorruption.Evaluate(percentage), 0.5f);
        OnCorruptionValueChange?.Invoke(Mathf.Abs(currentCorruption / maxCorruption), switchState);
    }
    public void GainSanity(float sanityValue)
    {
        bool switchState = false;
        if (currentCorruption == 0 && isCorrupted)
        {
            isCorrupted = false;
            switchState = true;
        }

        currentCorruption = Mathf.Max(currentCorruption - sanityValue, minCorruption);
        
        if (isCorrupted && currentCorruption < 0) currentCorruption = 0;
        float range = maxCorruption - minCorruption;
        float delta = currentCorruption - minCorruption;
        float percentage = delta / range;
        
        sprite.DOColor(colorCorruption.Evaluate(percentage), 0.5f);
        OnCorruptionValueChange?.Invoke(Mathf.Abs(currentCorruption / maxCorruption), switchState);
    }

    public void CollectFoolet(Color color)
    {
        maxJump++;
        fooletMat.SetColor("_GlowColor", color);
        getFooletEffects.Play();
        GainCorruption(0.75f);
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

        //Wall Jump
        if (context.performed)
        {
            if(isWallSliding() && wallJumpTimer > 0f)
            {
                Debug.Log("WallJump");
                isWallJumping = true;
                rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
                animator.SetTrigger("jump");

                wallJumpTimer = 0f;

                //forceFlip
                if (transform.localScale.x != wallJumpDirection)
                {
                    isFacingRight = !isFacingRight;
                    Vector3 ls = transform.localScale;
                    ls.x *= -1f;
                    transform.localScale = ls;
                    wallCheckOffset.x *= -1f;
                }

                Invoke(nameof(cancelWallJump), wallJumpTime);
                return;
            }
            if (IsGrounded() || (timeSinceGrounded < timeAllowedForJump && jumpRemaining == maxJump))
            {
                rb.linearVelocityY = jumpForce;
                isFirstJumpPressed = true;
                animator.SetTrigger("jump");
                StopAllCoroutines();
                StartCoroutine(handleFirstJumpTime());
                jumpRemaining--;
                return;
            }
            if(jumpRemaining > 0)
            {
                if (jumpRemaining == maxJump) jumpRemaining--;
                if (jumpRemaining == 0) return;
                animator.SetTrigger("jump");
                rb.linearVelocityY = doubleJumpForce;
                foreach (var effect in jumpEffects)
                {
                    effect.Play();
                }
                jumpRemaining--;
            }

        }
        
        if (context.canceled && rb.linearVelocityY > 0)
        {
            isFirstJumpPressed = false; // Stop coroutine with this bool
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
        while (isFirstJumpPressed)
        {
            jumpPressedTime += Time.deltaTime;
            if(jumpPressedTime >= jumpMaxPressTime)
            {
                isFirstJumpPressed = false;
                TMP.text = (timeToPressJump.Length+1).ToString();
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
                    TMP.text = (i+1).ToString();
                    yield return new WaitForSeconds(timeToPressJump[i] - jumpPressedTime);
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
            timeSinceGrounded = 0;
            return true;
        }
        else
        {
            timeSinceGrounded += Time.deltaTime;
            return false;
        }
    }    
    
    private bool IsBesideWall()
    {
        if (Physics2D.OverlapBox(wallCheckPos.position + (Vector3)wallCheckOffset, wallCheckSize, 0, wallLayer))
        {
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
