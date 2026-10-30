using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerAttack playerAttack;
    [Header("Speed")]
    [SerializeField] float speedOfPlayer = 10f;
    [SerializeField] float dashSpeedOfPlayer = 17f;
    [SerializeField] float dashDuration = 0.38f;
    [SerializeField] int maxDashs = 1;
    [Header("Jump")]
    [SerializeField] float jumpForceOfPlayer = 20f;
    [SerializeField] float shortHopJumpPlayerMultiplier = 0.5f;
    [SerializeField] float normalGravity = 5f;
    [SerializeField] float fallingGravity = 6f;
    [SerializeField] float wallSlidingSpeed = 3f;
    [SerializeField] float wallJumpForceX = 10f;
    [SerializeField] float wallJumpForceY = 20f;
    [SerializeField] float wallJumpControllDelayDuration = 0.1f;
    [SerializeField] int maxJumps = 1;
    [SerializeField] float jumpRefillLockDuration = 0.1f; // blocks ground refill right after a jump
    [SerializeField] float coyoteTime = 0.1f;             // grace window to still ground-jump after leaving a ledge
    [SerializeField] float maxFallSpeed = 20f;
    [SerializeField] float hardLand = 15f;
    [SerializeField] float softLand = 3f;
    [Header("CoolDowns")]
    [SerializeField] float dashCoolDownDuration = 0.5f;
    [Header("LedgeGrab")]
    [SerializeField] float LedgeClimbDuration = 0.65f;
    [SerializeField] Vector2 ledgeGrabOffSet = new Vector2(0, -1.44f);
    [SerializeField] Vector2 climbOffSet = new Vector2(0.85f, 1.65f);
    [SerializeField] float apexClipDuration = 0.625f;
    [SerializeField] private float pogoBounceForce = 12f;

    private float peakY;
    private bool wasRisingLastFrame;
    private bool isInApex = false;
    int jumpRemaining;
    int dashsRemaining;
    float jumpRefillLockTimer;
    float coyoteTimer;
    bool ledgePenaltyApplied;
    float dashCoolDownTimer = 0f;
    float fallStartY;
    bool wasAirBorne;
    Vector2 movement = new Vector2(0, 0);
    GroundDetector groundDetector;
    WallDetector wallDetector;
    Rigidbody2D myRigidBody2D;
    LedgeDetector ledgeDetector;
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] CameraFollowObject cameraFollowObject;

    public enum MovementStates
    {
        Grounded,
        AirBorne,
        WallSliding,
        WallJumping,
        Dashing,
        LedgeGrabbing,
        LedgeHanging,
        LedgeClimbing,
        LedgeDropping,
        Attacking
    }
    private MovementStates currentState = MovementStates.Grounded;
    private MovementStates lastAnimState;
    public bool CurrentStateIsAttacking => currentState == MovementStates.Attacking;
    public bool CurrentStateIsGrounded => currentState == MovementStates.Grounded;
    public bool IsFacingRight => transform.localScale.x > 0f;
    public float MoveInputY => movement.y;
    private float fallSpeedDampingYThreshold;

    private MovementStates ResolveGroundedState()
    {
        if (groundDetector.isGrounded)
            return MovementStates.Grounded;
        else if (wallDetector.GetWallDirection() != 0)
            return MovementStates.WallSliding;
        else
            return MovementStates.AirBorne;
    }
    private static readonly MovementStates[] commitedStates =
    {
        MovementStates.WallJumping,MovementStates.Dashing , MovementStates.LedgeGrabbing , MovementStates.LedgeDropping, MovementStates.LedgeClimbing , MovementStates.LedgeHanging
    };
    private bool MovementFuncGuard() => Array.IndexOf(commitedStates, currentState) >= 0;
    void Awake()
    {
        groundDetector = GetComponentInChildren<GroundDetector>();
        wallDetector = GetComponentInChildren<WallDetector>();
        ledgeDetector = GetComponentInChildren<LedgeDetector>();
    }
    void Start()
    {
        myRigidBody2D = GetComponent<Rigidbody2D>();
        fallSpeedDampingYThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
    }
    void Update()
    {
        if ((currentState == MovementStates.AirBorne || currentState == MovementStates.WallSliding) && ledgeDetector.IsLedgeAvailable())
        {
            GrabLedge();
        }
        if (currentState == MovementStates.Grounded || currentState == MovementStates.AirBorne || currentState == MovementStates.WallSliding)
        {
            currentState = ResolveGroundedState();
        }
        if (dashCoolDownTimer > 0f) dashCoolDownTimer -= Time.deltaTime;
        MovementFunc();
        ApplyFallGravity();
        CameraDampingFunc();
        CheckLanding();
        CheckApex();
        UpdateAnimator();
    }

    private void CameraDampingFunc()
    {
        // if we are falling past a certain speed threshold
        if (myRigidBody2D.linearVelocityY < fallSpeedDampingYThreshold &&
            !CameraManager.instance.IsLerpingYDamping &&
            !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        // if we are standing still or moving up
        if (myRigidBody2D.linearVelocityY >= 0f &&
            !CameraManager.instance.IsLerpingYDamping &&
            CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }
    }

    private void UpdateAnimator()
    {
        //attack
        animator.SetBool("IsAttacking", playerAttack.IsAttacking);
        //
        float myRbFallSpeed = myRigidBody2D.linearVelocityY;
        ContiniousParamsAnimator(myRbFallSpeed);
        int id = currentState switch
        {
            MovementStates.Grounded => movement.x != 0 ? 1 : 0,
            MovementStates.AirBorne => myRbFallSpeed > -1 ? 2 : 3,
            MovementStates.WallSliding => 4,
            MovementStates.WallJumping => 2,
            MovementStates.LedgeHanging => 5,
            MovementStates.Dashing => 6,
            _ => animator.GetInteger("StateID")
        };

        if (currentState == lastAnimState && animator.GetInteger("StateID") == id)
            return;

        animator.SetInteger("StateID", id);

        if (currentState == MovementStates.WallSliding &&
            lastAnimState != MovementStates.WallSliding)
        {
            animator.SetTrigger("EnterWallHold");
        }

        lastAnimState = currentState;
    }

    private void ContiniousParamsAnimator(float rbFallSpeed)
    {
        animator.SetFloat("VelocityY", rbFallSpeed);
        bool inApex = isInApex;
        animator.SetBool("InApex", inApex);
        bool inLand = animator.GetCurrentAnimatorStateInfo(0).IsName("Land") || animator.GetCurrentAnimatorStateInfo(0).IsName("LandFromDistance");
        animator.SetBool("InLand", inLand);
        bool inClimbing = currentState == MovementStates.LedgeClimbing;
        animator.SetBool("InClimbing", inClimbing);
    }

    private void MovementFunc()
    {

        bool grounded = groundDetector.isGrounded;
        bool onWall = wallDetector.GetWallDirection() != 0;

        if (jumpRefillLockTimer > 0f) jumpRefillLockTimer -= Time.deltaTime;

        // i set ground dashes cost free
        if (grounded || onWall)
            dashsRemaining = maxDashs;

        // jump refill: blocked for a moment after a jump, because the ground
        // check is still true for a few frames while the player is leaving the floor
        if ((grounded || onWall) && jumpRefillLockTimer <= 0f)
            jumpRemaining = maxJumps;

        // coyote time + "walking off a ledge costs one jump"
        if (grounded)
        {
            coyoteTimer = coyoteTime;
            ledgePenaltyApplied = false;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            if (coyoteTimer <= 0f && !ledgePenaltyApplied && !onWall)
            {
                ledgePenaltyApplied = true;
                if (jumpRemaining >= maxJumps) jumpRemaining = maxJumps - 1;
            }
        }
        if (MovementFuncGuard()) return;


        FlipSprite();
        myRigidBody2D.linearVelocityX = movement.x * speedOfPlayer;
    }
    private void ApplyFallGravity()
    {
        if (MovementFuncGuard()) return;
        if (currentState == MovementStates.WallSliding)
        {
            if (myRigidBody2D.linearVelocityY < -wallSlidingSpeed)
            {
                myRigidBody2D.linearVelocityY = -wallSlidingSpeed;
            }
            return;
        }
        if (myRigidBody2D.linearVelocityY < -maxFallSpeed)
        {
            myRigidBody2D.linearVelocityY = -maxFallSpeed;
        }
        GravityScaleForFalling();
    }
    private void FlipSprite()
    {
        if (movement.x > 0)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
            cameraFollowObject.CallTurn();
        }
        else if (movement.x < 0)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
            cameraFollowObject.CallTurn();
        }
    }

    private void GravityScaleForFalling()
    {
        if (myRigidBody2D.linearVelocityY < 0)
        {
            myRigidBody2D.gravityScale = fallingGravity;
        }
        else
        {
            myRigidBody2D.gravityScale = normalGravity;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        movement.x = input.x;
        movement.y = input.y;
        if (currentState == MovementStates.LedgeHanging)
        {
            if (movement.y > 0 || movement.x * wallDetector.GetWallDirection() < 0f)
            {
                StartLedgeClimb();
            }
            else if (movement.y < 0)
            {
                ExitToGroundedState();
                // insteaed of airborne i set it to normal check;
            }
            return;
        }
    }
    // ledge grab
    private void StartLedgeClimb()
    {
        if (currentState != MovementStates.LedgeHanging) return;

        currentState = MovementStates.LedgeClimbing;
        animator.SetTrigger("StartLedgeClimb");
        StartCoroutine(LedgeClimbRoutine());
    }

    private void GrabLedge()
    {
        float wallDir = wallDetector.GetWallDirection();
        if (wallDir == 0f) return; // wall lost, don't grab

        // Freeze physics
        myRigidBody2D.linearVelocity = Vector2.zero;
        myRigidBody2D.gravityScale = 0f;

        // Face the wall (keep your original sign if it works on the right side)
        float facing = -Mathf.Sign(wallDir);
        transform.localScale = new Vector3(
            facing * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z);

        // Mirror the x offset by facing so both sides work
        Vector2 offset = new Vector2(ledgeGrabOffSet.x * facing, ledgeGrabOffSet.y);
        transform.position = (Vector2)ledgeDetector.ledgeCheckPoint.position + offset;

        currentState = MovementStates.LedgeHanging;
    }

    private IEnumerator LedgeClimbRoutine()
    {
        // Stop physics from fighting the scripted movement
        myRigidBody2D.linearVelocity = Vector2.zero;
        myRigidBody2D.bodyType = RigidbodyType2D.Kinematic;

        float duration = Mathf.Max(LedgeClimbDuration, 0.01f);
        float facing = Mathf.Sign(transform.localScale.x);

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(facing * climbOffSet.x, climbOffSet.y, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.position = endPos;
        animator.ResetTrigger("StartLedgeClimb");
        ExitToGroundedState();
    }

    // dash
    public void OnDash(InputAction.CallbackContext context)
    {
        float direction = Mathf.Sign(transform.localScale.x);
        if (context.performed && dashCoolDownTimer <= 0f && dashsRemaining > 0)
        {
            dashCoolDownTimer = dashCoolDownDuration;
            dashsRemaining--;
            currentState = MovementStates.Dashing;
            myRigidBody2D.linearVelocityX = direction * dashSpeedOfPlayer;
            myRigidBody2D.linearVelocityY = 0f;
            myRigidBody2D.gravityScale = 0f;
            trailRenderer.emitting = true;
            Invoke(nameof(ExitToGroundedState), dashDuration);
        }
    }
    // jumping
    public void OnJump(InputAction.CallbackContext context)
    {
        if (currentState == MovementStates.LedgeHanging && context.performed)
        {
            StartLedgeClimb();
            return;
        }
        if (context.performed && jumpRemaining > 0 && currentState != MovementStates.Dashing)
        {
            JumpTry();
        }
        if (context.canceled && myRigidBody2D.linearVelocityY > 0)
        {
            // apply short jump
            myRigidBody2D.linearVelocityY *= shortHopJumpPlayerMultiplier;
        }
    }
    private void JumpTry()
    {
        float wallDir = wallDetector.GetWallDirection();
        if (groundDetector.isGrounded || coyoteTimer > 0f)
        {
            myRigidBody2D.linearVelocityY = jumpForceOfPlayer;
            currentState = MovementStates.AirBorne;
        }
        else if (wallDir != 0)
        {
            WallJump(wallDir);
        }
        else
        {
            myRigidBody2D.linearVelocityY = jumpForceOfPlayer;
            currentState = MovementStates.AirBorne;
        }
        jumpRemaining--;
        jumpRefillLockTimer = jumpRefillLockDuration;
        coyoteTimer = 0f;
    }

    // jumping - wall jump
    private void WallJump(float direction)
    {
        currentState = MovementStates.WallJumping;
        jumpRemaining = maxJumps;
        myRigidBody2D.linearVelocityX = direction * wallJumpForceX;
        transform.localScale = new Vector3(Mathf.Sign(direction), transform.localScale.y, transform.localScale.z);
        myRigidBody2D.linearVelocityY = wallJumpForceY;
        Invoke(nameof(EndWallJump), wallJumpControllDelayDuration);
    }

    private void EndWallJump()
    {
        currentState = ResolveGroundedState();
    }
    private void CheckLanding()
    {
        bool isAirBorneNow = currentState == MovementStates.AirBorne;
        if (isAirBorneNow && !wasAirBorne) fallStartY = transform.position.y;
        if (wasAirBorne && groundDetector.isGrounded)
        {
            float fallDistance = fallStartY - transform.position.y;
            if (fallDistance > hardLand) animator.SetTrigger("EnterLandFromDistance");
            else if (fallDistance > softLand) animator.SetTrigger("EnterLand");
        }
        wasAirBorne = isAirBorneNow;
    }

    private void CheckApex()
    {
        if (currentState != MovementStates.AirBorne)
        {
            wasRisingLastFrame = false;
            return;
        }

        bool isRisingNow = myRigidBody2D.linearVelocityY > 0;

        if (wasRisingLastFrame && !isRisingNow && !isInApex)
        {
            isInApex = true;
            Invoke(nameof(EndApex), apexClipDuration);
        }

        wasRisingLastFrame = isRisingNow;
    }

    private void EndApex()
    {
        isInApex = false;
    }

    private void ExitToGroundedState()
    {
        myRigidBody2D.bodyType = RigidbodyType2D.Dynamic;
        trailRenderer.emitting = false;
        myRigidBody2D.gravityScale = normalGravity;
        currentState = ResolveGroundedState();
    }
    public void ExitToGroundedStatePublic()
    {
        ExitToGroundedState();
    }
    public void PogoBounce()
    {
        myRigidBody2D.linearVelocityY = pogoBounceForce;
        jumpRemaining = maxJumps;
    }
    public void DashUnlock()
    {
        maxDashs = 1;
    }
}
