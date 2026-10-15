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
    [SerializeField] float wallJumpForceX = 10f;
    [SerializeField] float wallJumpForceY = 20f;
    [SerializeField] float wallJumpControllDelayDuration = 0.1f;
    [SerializeField] int maxJumps = 1;
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

    private float peakY;
    private bool wasRisingLastFrame;
    private bool isInApex = false;
    int jumpRemaining;
    int dashsRemaining;
    float dashCoolDownTimer = 0f;
    float fallStartY;
    bool wasAirBorne;
    Vector2 movement = new Vector2(0, 0);
    GroundDetector groundDetector;
    WallDetector wallDetector;
    Rigidbody2D myRigidBody2D;
    LedgeDetector ledgeDetector;

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
    public float MoveInputY => movement.y;
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
        CheckLanding();
        CheckApex();
        UpdateAnimator();
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

        if (groundDetector.isGrounded || wallDetector.GetWallDirection() != 0)
        {
            jumpRemaining = maxJumps;
            dashsRemaining = maxDashs;
        }
        if (MovementFuncGuard()) return;


        FlipSprite();
        myRigidBody2D.linearVelocityX = movement.x * speedOfPlayer;
    }
    private void ApplyFallGravity()
    {
        if (MovementFuncGuard()) return;
        if (myRigidBody2D.linearVelocityY < -maxFallSpeed)
        {
            myRigidBody2D.linearVelocityY = -maxFallSpeed;
        }
        GravityScaleForFalling();
    }
    private void FlipSprite()
    {
        if (movement.x > 0)
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        else if (movement.x < 0)
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
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
        currentState = MovementStates.LedgeGrabbing;
        myRigidBody2D.linearVelocity = Vector2.zero;
        myRigidBody2D.gravityScale = 0f;

        float wallDir = wallDetector.GetWallDirection();
        transform.localScale = new Vector3(-Mathf.Sign(wallDir), transform.localScale.y, transform.localScale.z);

        Vector2 pos = (Vector2)ledgeDetector.ledgeCheckPoint.position + ledgeGrabOffSet;
        transform.position = pos;
        currentState = MovementStates.LedgeHanging;
    }
    private IEnumerator LedgeClimbRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(Mathf.Sign(transform.localScale.x) * climbOffSet.x, climbOffSet.y, 0);

        float elapsed = 0f;
        while (elapsed < LedgeClimbDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / LedgeClimbDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPos, endPos, smoothT);
            yield return null;
        }

        transform.position = endPos;
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
        if (groundDetector.isGrounded)
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
        myRigidBody2D.gravityScale = normalGravity;
        currentState = ResolveGroundedState();
    }
    public void ExitToGroundedStatePublic()
    {
        ExitToGroundedState();
    }
}
