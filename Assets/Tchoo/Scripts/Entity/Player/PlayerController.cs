using DG.Tweening;
using NaughtyAttributes;
using System;
using System.Collections;
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
    [SerializeField] private float jumpForce;
    [SerializeField] private float[] jumpHoldDurations;    
    [SerializeField] private float[] minimunJumpForceForDurations;
    [SerializeField, Range(0, 1)] private float decelerationValue;
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
    public float CurrentCorruption => currentCorruption;
    [Header("Damages")]
    [SerializeField] private Vector2 damageEjectionPower = new(3f, 3f);
    [SerializeField] private float invulnerabilityTime = 1f;
    [SerializeField] private float damageTime = 0.5f;
    private float _invulnerabilityTimer = 0;
    private float _damageTimer = 0;

    [Header("Effects")]
    [SerializeField] private ParticleSystem[] jumpEffects;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Light2D[] lights;
    [SerializeField] private ParticleSystem getFooletEffects;
    [SerializeField] private ParticleSystem getHitEffect;
    [SerializeField] private Material fooletMat;
    [SerializeField] private Animator animator;

    [Header("Debuging")]
    [SerializeField] private TextMeshPro debugText;
    //[SerializeField] private Image imgGround;
    [SerializeField] private Image imgWall;
    [SerializeField] private CanvasGroup canvasGroup;
    private Color debugColor = new(0f, 0f, 0f, 0f);

    bool isWallJumping;
    bool isTurningOnGround = false;
    //bool isInvulnerable = false;

    public event Action<float, bool> OnCorruptionValueChange;
    public event Action<float> OnDirectionXChange;
    public event Action<float> OnDirectionYChange;
    //[SerializeField] private bool isWallSliding;

    public float horizontalMovement;
    public float verticalMovement;
    private float jumpPressedTime;
    private float jumpPressedTimeDelta;
    private bool isHoldingJump;
    private bool isFacingRight = true;
    private float timeSinceGrounded = 0f;
    private float timeAllowedForJump = 0.22f;
    private bool isCorrupted = false;
    private bool isInFirstJumpAscent = false;
    private bool endFirstJump = false;
    private float jumpMaxPressTime;
    [SerializeField, Range(0.05f, 2f)] private float turnSpeed = 0.25f;

    private void Start()
    {
        jumpMaxPressTime = jumpHoldDurations[jumpHoldDurations.Length - 1];
        Debug.Log(jumpMaxPressTime.ToString());
        sprite.color = colorCorruption.Evaluate(0f);
        foreach (var item in lights)
        {
            item.color = colorCorruption.Evaluate(0f);
        }
        currentCorruption = minCorruption;
        UpdateCorruption(false, false);
        //OnDirectionXChange += Flip;
    }

    void Update()
    {
        if (IsGrounded()) jumpRemaining = maxJump;
        processWallJump();
        ProcessDamage();
        //if (IsGrounded()) jumpRemaining = maxJump;
        //processWallJump();
        //ProcessGravity();
        if (!isWallJumping && _damageTimer <= 0)
        {
            //UpdateMovement();
            if (isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
            {
                //OnDirectionXChange?.Invoke(transform.localScale.x * -1);
                Flip();
            }
        }
        animator.SetFloat("yVelocity", rb.linearVelocityY);
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        //Debug.Log( rb.linearVelocity.magnitude);
        //animator.SetFloat("WalkSpeed", Mathf.Abs(horizontalMovement));
    }

    private void FixedUpdate()
    {
        if (!isWallJumping && _damageTimer <= 0)
        {
            UpdateMovement();
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

    private void UpdateMovement()
    {
        if (!isTurningOnGround)
        {
            rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, moveSpeed * horizontalMovement, moveSpeed * turnSpeed);
        }
        rb.linearVelocityY = isWallSliding() 
            ? Mathf.Clamp(rb.linearVelocityY, wallSlideSpeed*-1f, jumpForce) 
            : Mathf.Clamp(rb.linearVelocityY, GetFallSpeed(), jumpForce);
    }

    public void SwitchSettings(JumpSettings settings)
    {
        jumpForce = settings.JumpForce;
        jumpHoldDurations = settings.JumpHoldDurations;
        minimunJumpForceForDurations = settings.MinimumJumpForceForDurations;
        baseGravity = settings.BaseGravity;
        gravityMultiplier = settings.GravityMultiplier;
        decelerationValue = settings.DecelerationValue;
    }

    private void ProcessDamage()
    {
        if (_invulnerabilityTimer > 0)
        {
            _invulnerabilityTimer -= Time.deltaTime;
        }
        if (_damageTimer > 0)
        {
            _damageTimer -= Time.deltaTime;
        }

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

    public void TakeDamage(float amount, Vector2 from)
    {
        if (_invulnerabilityTimer > 0)
        {
            Debug.Log("isInvunerable : can't take dmg");
            return;
        }
        float direction;
        if (from.x > transform.position.x) direction = -1; 
        else direction = 1;
        
        rb.linearVelocity = new Vector2(direction * damageEjectionPower.x, damageEjectionPower.y);
        animator.SetTrigger("getHit");
        getHitEffect.Play();

        GainCorruption(amount);
        _damageTimer += damageTime;
        _invulnerabilityTimer += invulnerabilityTime;
    }

    public void GainCorruption(float corruptionValue)
    {
        if (_invulnerabilityTimer > 0)
        {
            Debug.Log("isInvunerable : can't take dmg");
            return;
        }
        Debug.Log("GainCorruption");
        bool switchState = false;
        
        currentCorruption = Mathf.Min(currentCorruption + corruptionValue, maxCorruption);

        if (!isCorrupted && currentCorruption >= 0)
        {
            currentCorruption = 0;
            isCorrupted = true;
            switchState = true;
            SetInvunerability(2f);
        }
        UpdateCorruption(switchState, true);
    }

    public void GainSanity(float sanityValue)
    {
        Debug.Log("GainSanity");
        bool switchState = false;

        currentCorruption = Mathf.Max(currentCorruption - sanityValue, minCorruption);

        if (isCorrupted && currentCorruption <= 0)
        {
            currentCorruption = 0;
            isCorrupted = false;
            switchState = true;
            SetInvunerability(2f);
        }
        UpdateCorruption(switchState, false);
    }

    private void UpdateCorruption(bool switchState, bool DoColor)
    {
        float range = maxCorruption - minCorruption;
        float delta = currentCorruption - minCorruption;
        float percentage = delta / range;
        OnCorruptionValueChange?.Invoke(percentage, switchState);
        if (DoColor)
        {
            sprite.DOColor(colorCorruption.Evaluate(1), 0.05f).OnComplete(() =>
            sprite.DOColor(colorCorruption.Evaluate(percentage), 0.45f).SetEase(Ease.InSine));
        }
        else
        {
            sprite.DOColor(colorCorruption.Evaluate(percentage), 0.5f).SetEase(Ease.InSine);
        }
    }

    private void SetInvunerability(float amount)
    {
        _invulnerabilityTimer += amount;
    }

    //private IEnumerator Invulnerability(float time)
    //{
    //    Debug.Log("Start coroutine invulnerability : " + time);
    //    isInvulnerable = true;
    //    yield return new WaitForSeconds(time);
    //    isInvulnerable = false;
    //    Debug.Log("End coroutine invulnerability");
    //}

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

    private void ResetAllTriggers()
    {
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(param.name);
            }
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
                    Flip();
                }

                Invoke(nameof(cancelWallJump), wallJumpTime);
                return;
            }
            if (IsGrounded() || (timeSinceGrounded < timeAllowedForJump && jumpRemaining == maxJump))
            {
                Debug.Log("-------- start Jump --------");
                rb.linearVelocityY = jumpForce;
                isHoldingJump = true;
                animator.SetTrigger("jump");
                StopAllCoroutines();
                jumpPressedTime = 0;
                jumpPressedTimeDelta = 0;
                isInFirstJumpAscent = true;
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
                return;
            }

        }
        
        if (context.canceled && rb.linearVelocityY > 0)
        {
            isHoldingJump = false; // Stop coroutine with this bool
            Debug.Log("--- jumpPressedTime onStop --- " + jumpPressedTime);
            jumpPressedTimeDelta = 0;
        }



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

                Debug.Log("VelocityY => " + rb.linearVelocityY +
                    " | pressedTime => " + jumpPressedTime +
                    " | Delta => " + jumpPressedTimeDelta +
                    " | Jump n�" + (i + 1).ToString()
                    );

                break;
            }
        }

        if (isHoldingJump)
        {
            jumpPressedTime += Time.fixedDeltaTime;
            //If holding time reach the maximum
            if (jumpPressedTime >= jumpHoldDurations[jumpHoldDurations.Length-1])
            {
                //Force Jump button released
                isHoldingJump = false;
                jumpPressedTimeDelta = 0;
                isInFirstJumpAscent = false;
                debugText.text = jumpHoldDurations.Length.ToString();
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
        for (int i = 0; i < jumpHoldDurations.Length; i++)
        {
            //Find the next jump height level
            if (jumpPressedTime < jumpHoldDurations[i])
            {
                debugText.text = (i + 1).ToString();
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
            Debug.Log("deceleration velocity : " + rb.linearVelocityY.ToString());
        }
        else
        {
            Debug.Log("end deceleration velocity : " + rb.linearVelocityY.ToString());
            if (rb.linearVelocityY > -0.5f) rb.linearVelocityY = 0;
            Debug.Log("end deceleration velocity : " + rb.linearVelocityY.ToString());
            endFirstJump = false;
        }
    }

    //private IEnumerator handleFirstJumpTimeOLD()
    //{

    //    jumpPressedTime = 0;
    //    while (isFirstJumpPressed)
    //    {
    //        jumpPressedTime += Time.deltaTime;
    //        if(jumpPressedTime >= jumpMaxPressTime)
    //        {
    //            isFirstJumpPressed = false;
    //            TMP.text = timeToPressJump.Length.ToString();
    //            StartCoroutine(stopPositiveVelocityY());
    //            yield return new WaitForSeconds(0.5f);
    //            TMP.text = "";
    //            yield break;
    //        }
    //        yield return null;
    //    }
    //    if(jumpPressedTime < jumpMaxPressTime)
    //    {
    //        for(int i = 0; i < timeToPressJump.Length; i++)
    //        {
    //            if (jumpPressedTime < timeToPressJump[i])
    //            {
    //                TMP.text = (i+1).ToString();
    //                Debug.Log("---");
    //                Debug.Log((i + 1).ToString());
    //                Debug.Log(jumpPressedTime);
    //                //yield return new WaitForFixedUpdate();
    //                yield return new WaitForSeconds(timeToPressJump[i] - jumpPressedTime);
    //                StartCoroutine(stopPositiveVelocityY());
    //                yield break;
    //            }
    //        }
    //        Debug.LogError($"/! JumpPressTime = {jumpPressedTime} & jumpPressTimeMax = {jumpMaxPressTime}, but the coroutine didnt do the condition to wait /!");
    //    }
    //    yield return null;
    //}

    //private IEnumerator stopPositiveVelocityYOLD()
    //{
    //    Debug.LogWarning("position Y when stop => " + transform.position.y);
    //    while (rb.linearVelocityY > 0.1f)
    //    {
    //        rb.linearVelocityY = Mathf.Min(Mathf.MoveTowards(rb.linearVelocityY, 0f, 0.09f * Time.deltaTime),jumpForce/2f);
    //        yield return null;
    //    }
    //    rb.linearVelocityY = 0;
    //    yield return new WaitForSeconds(0.3f);
    //    debugText.text = "";
    //}

    public void OpenMenu(InputAction.CallbackContext context)
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        Time.timeScale = 0f;
    }
    //public void CloseMenu(InputAction.CallbackContext context)
    //{
    //    canvasGroup.alpha = 0;
    //    canvasGroup.interactable = false;
    //    canvasGroup.blocksRaycasts = false;
    //    Time.timeScale = 1f;
    //}


    private bool IsGrounded()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position + (Vector3)groundCheckOffset, groundCheckSize, 0, groundLayer))
        {
            //if(timeSinceGrounded > 0)
            //{
            //    OnDirectionYChange?.Invoke(1f);
            //}
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
            //imgWall.color = Color.blue;
            return true;
        }
        //imgWall.color = debugColor;
        return false;
    }

    private void Flip()
    {
        if (IsGrounded() && rb.linearVelocityY < 0.01f)
        {
            animator.SetTrigger("turn");
        }
        else
        {
            EndFlip();
        }
        //isFacingRight = !isFacingRight;
        //Vector3 ls = transform.localScale;
        //ls.x = direction;
        //transform.localScale = ls;
        //wallCheckOffset.x = direction;
        //groundCheckOffset.x = direction;
    }

    /// <summary>
    /// Called by animator after flip (2keyframes)
    /// </summary>
    private void EndFlip()
    {
        isFacingRight = !isFacingRight;
        Vector3 ls = transform.localScale;
        ls.x *= -1f;
        transform.localScale = ls;
        wallCheckOffset.x *= -1f;
        groundCheckOffset.x *= -1f;
    }

    private void ChangeCameraDirection() {
        OnDirectionXChange?.Invoke(transform.localScale.x);
    }

    private float GetFallSpeed()
    {
        if(verticalMovement > 0.4f)
        {
            return fallMinSpeed;
        }
        if (verticalMovement < -0.4f)
        {
            //OnDirectionYChange?.Invoke(-1f);
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
