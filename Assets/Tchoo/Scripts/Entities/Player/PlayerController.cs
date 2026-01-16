using DG.Tweening;
using NaughtyAttributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    //public Rigidbody2D rb;
    //public Animator animator;
    //public SpriteRenderer spriteRenderer;
    public PlayerMovementHandler movementHandler;
    public PlayerAnimationHandler animationHandler;
    //public PlayerCorruptionHandler corruptionHandler;
    
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
    [SerializeField] private ParticleSystem wallJumpEffect;
    [SerializeField] private ParticleSystem landingEffect;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Light2D[] lights;
    [SerializeField] private ParticleSystem getFooletEffects;
    [SerializeField] private ParticleSystem getHitEffect;
    [SerializeField] private Material fooletMat;
    [SerializeField] private Animator animator;
    [SerializeField] private Image imageWallJump;
    [SerializeField] private Image imageDoubleJump;
    [SerializeField] private Image imageEnd;
    [SerializeField] private AnimationEffectTriggers animationEffect;

    [Header("Debuging")]
    [SerializeField] private TextMeshPro debugText;
    [SerializeField] private PauseMenu pauseMenu;

    public event Action<float, bool> OnCorruptionValueChange;
    public event Action<float> OnDirectionXChange;
    public event Action<float> OnDirectionYChange;
    //public event Action<bool> OnJumpPressed;

    private bool updateCameraDown = false;
    public float horizontalMovement;
    public float verticalMovement;
    private bool isFacingRight = true;
    private bool isHoldingJump;
    private bool isGrounded;

    private bool isCorrupted = false;

    //[SerializeField, Range(0.05f, 2f)] private float turnSpeed = 0.25f;

    private void Start()
    {
        sprite.color = colorCorruption.Evaluate(0f);
        foreach (var item in lights)
        {
            item.color = colorCorruption.Evaluate(0f);
        }
        currentCorruption = minCorruption;
        UpdateCorruption(false, false);
        Subscribe();
    }

    private void Subscribe()
    {
        movementHandler.OnGroundedStateChanged += HandleGroundedChange;
        movementHandler.OnGroundedStateChanged += animationHandler.HandleGroundedStateChange;
        movementHandler.OnFlipRequested += Flip;
        movementHandler.OnJumpProcessed += PlayJumpEffects;
    }

    void Update()
    {
        movementHandler.HandleMovementInput(horizontalMovement, verticalMovement,  isHoldingJump);
        ProcessDamage();
        if (!movementHandler.IsWallJumping && _damageTimer <= 0)
        {
            if (isFacingRight && horizontalMovement < -0.1f || !isFacingRight && horizontalMovement > 0.1f)
            {
                Flip();
            }
        }
        //animator.SetFloat("yVelocity", movementHandler.CurrentVelocity.y);
        //animator.SetFloat("magnitude", movementHandler.CurrentVelocity.magnitude);
    }

    private void FixedUpdate()
    {
        movementHandler.ApplyMovement(); // uses stored inputs
        CheckFallingSpeed();
        animationHandler.SetVelocity(movementHandler.CurrentVelocity);
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
        if (horizontalMovement > 0.55f)
        {
            horizontalMovement = 1f;
        }
        else if (horizontalMovement > 0.1f)
        {
            horizontalMovement = 0.4f;
        }
        else if (horizontalMovement < -0.55f)
        {
            //Debug.Log(horizontalMovement);
            horizontalMovement = -1f;
        }
        else if (horizontalMovement < -0.1f)
        {
            //Debug.Log(horizontalMovement);
            horizontalMovement = -0.4f;
        }
        else
        {
            horizontalMovement = 0f;
        }


        verticalMovement = context.ReadValue<Vector2>().y;
        if (verticalMovement > 0.33f)
        {
            verticalMovement = 1;
        }
        if (verticalMovement < -0.33f)
        {
            verticalMovement = -1;
        }
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isHoldingJump = true;
            movementHandler.TryProcessJump();
            //OnJumpPressed?.Invoke(isHoldingJump);
            //ProcessJump();
        }

        if (context.canceled)
        {
            isHoldingJump = false; // Stop coroutine with this bool
            movementHandler.ReleaseJumpButton();

            //Debug.Log("--- jumpPressedTime onStop --- " + jumpPressedTime);
            //jumpPressedTimeDelta = 0;
        }
    }

    

    public void UpdateCorruptionRange(float min, float max)
    {
        minCorruption = min;
        maxCorruption = max;
    }

    private void HandleGroundedChange(bool grounded)
    {
        Debug.Log("IS GROUNDED " + grounded);
        if(grounded)
        {
            PlayLandingEffect();
        }
        isGrounded = grounded;
    }




    private void CheckFallingSpeed()
    {
        if(updateCameraDown && movementHandler.CurrentVelocity.y > -0.01f)
        {
            updateCameraDown = false;
            OnDirectionYChange?.Invoke(1);
            //Debug.Log("DIRECTION CHANGE : POSITIVE");
        }
        else if(!updateCameraDown && movementHandler.IsFallingFast()) 
        { 
            updateCameraDown = true;
            OnDirectionYChange?.Invoke(-1);
            //Debug.Log("DIRECTION CHANGE : NEGATIVE");

        }
    }



    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isGrounded)
            {
                animationHandler.TryTriggerAnim(EPlayerAnim.Attack);
            }
            else
            {
                animationHandler.TryTriggerAnim(EPlayerAnim.AerialAttack);
            }
        }
    }

    public void Purify(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animationHandler.TryTriggerAnim(EPlayerAnim.Purify);
        }
    }


    private Tween levitateTween;
    private float animationFrames = 4;
    private float sampleRate = 24; // Sample rate de l’animation

    public void Levitate(float distance)
    {
        if (levitateTween != null && levitateTween.IsActive()) return;

        float targetY = transform.position.y + distance;
        float duration = animationFrames / sampleRate;

        movementHandler.StopGravity(true);

        //baseGravity = 0.0f; // Réduction de la gravité
        Debug.Log("Levitate duration : " + duration);
        levitateTween = transform.DOMoveY(targetY, duration)
                                 .SetEase(Ease.OutQuad).OnComplete(() => { StopLevitate(); });
    }

    public void StopLevitate()
    {
        levitateTween?.Kill();
        movementHandler.StopGravity(false);
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

    public void TakeDamage(float amount, Vector2 from, bool bypassInvulnerability = false)
    {
        if (_invulnerabilityTimer > 0 && !bypassInvulnerability)
        {
            Debug.Log("isInvunerable : can't take dmg");
            return;
        }
        float direction;
        if (from.x > transform.position.x) direction = -1; 
        else direction = 1;
        
        movementHandler.EjectPlayer(new Vector2(direction * damageEjectionPower.x, damageEjectionPower.y));
        animationHandler.TryTriggerAnim(EPlayerAnim.GetHit);
        TakeDamage(amount, bypassInvulnerability);
    }

    public void TakeDamage(float amount, bool bypassInvulnerability = false)
    {
        if (_invulnerabilityTimer > 0 && !bypassInvulnerability)
        {
            Debug.Log("isInvunerable : can't take dmg");
            return;
        }
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

        if(currentCorruption >= maxCorruption)
        {
            GameOver();
        }

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
        OnCorruptionValueChange?.Invoke(currentCorruption, switchState);
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

    [Obsolete]
    public void CollectFoolet(Color baseColor, Color glowColor, Power power)
    {
        fooletMat.SetColor("_GlowColor", glowColor);
        getFooletEffects.startColor = baseColor;
        getFooletEffects.Play();
        GainCorruption(0.5f);
        if (power == Power.DoubleJump)
        {
            movementHandler.IncreaseMaxJump();
            imageDoubleJump.gameObject.SetActive(true);
            imageDoubleJump.transform.DOScale(3, 0.3f).OnComplete(() => { 
                imageDoubleJump.transform.DOScale(1, 1.5f).SetEase(Ease.OutCubic);
                imageDoubleJump.DOFade(1, 1.5f);
            });


        }
        if (power == Power.WallJump)
        {
            movementHandler.UnlockWallJump(true);
            imageWallJump.gameObject.SetActive(true);
            imageWallJump.transform.DOScale(3, 0.3f).OnComplete(() => {
                imageWallJump.transform.DOScale(1, 1.5f).SetEase(Ease.OutCubic);
                imageWallJump.DOFade(1, 1.5f);
            });
        }
        if (power == Power.End)
        {
            imageEnd.gameObject.SetActive(true);
            imageEnd.transform.DOScale(3, 0.3f).OnComplete(() => {
                imageEnd.transform.DOScale(1, 1.5f).SetEase(Ease.OutCubic);
                imageEnd.DOFade(1, 1.5f);
            });
        }
    }

    private void PlayLandingEffect()
    {
        landingEffect.Play();
    }

    public void ResetPower()
    {
        //Provisoire, gagne en sanité pour éviter que la corruption soit visible
        GainSanity(2);
        //Reset
        movementHandler.ResetMaxJump();
        movementHandler.UnlockWallJump(false);
        //Enlève l'image avec un fade out
        imageDoubleJump.DOFade(0, 1f).OnComplete(() => { imageDoubleJump.gameObject.SetActive(false); });
        imageWallJump.DOFade(0,1f).OnComplete(() => { imageWallJump.gameObject.SetActive(false); });
        imageEnd.DOFade(0,1f).OnComplete(() => { imageEnd.gameObject.SetActive(false); });
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

    public void OpenMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            pauseMenu.PauseGame();
        }
    }

    private void PlayJumpEffects(JumpType jumpType)
    {
        if (jumpType == JumpType.None) return;

        if (jumpType == JumpType.DoubleJump)
        {
            foreach (var effect in jumpEffects)
            {
                effect.Play();
            }
        }

        if (jumpType == JumpType.WallJump)
        {
            wallJumpEffect.Play();
        }
    }

    //Called by animator event
    private void PlayAnimationEffect(AnimationEffect effect)
    {
        Debug.Log("Play anim " + effect.ToString());
        animationEffect.PlayAnim(effect);
    }
    
    private void Flip()
    {
        if (isGrounded && movementHandler.CurrentVelocity.y < 0.01f)
        {
            animationHandler.TryTriggerAnim(EPlayerAnim.Turn);
            //animator.SetTrigger("turn");
        }
        else
        {
            EndFlip();
        }
    }

    /// <summary>
    /// Called by animator after flip (2keyframes)
    /// </summary>
    private void EndFlip()
    {
        isFacingRight = !isFacingRight;

        //Flip visual
        Vector3 ls = transform.localScale;
        ls.x *= -1f;
        transform.localScale = ls;

        //Notify others
        OnDirectionXChange?.Invoke(ls.x);
        movementHandler.FlipCheckOffsets(ls.x);
        debugText.transform.localScale = ls;
        wallJumpEffect.transform.localScale = ls;
    }

    private void GameOver()
    {
        SetInvunerability(4f);
        movementHandler.StopGravity(true);
        //animator.SetTrigger("GameOver");
        Debug.Log("Animation Death..");
        animationHandler.TryTriggerAnim(EPlayerAnim.GameOver);
    }

    //Called by gameOver animation event
    private void OnGameOverAnimationComplete()
    {
        Debug.Log("pause without resume");
        movementHandler.StopGravity(false);
        pauseMenu.GameOver();
        animator.Play("Idle", 0, 0f);
    }
}
