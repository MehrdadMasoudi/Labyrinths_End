using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] float speedOfPlayer = 5f;
    [SerializeField] float dashSpeedOfPlayer = 10f;
    [SerializeField] float dashDuration = 0.3f;
    [SerializeField] int maxDashs = 1;
    [Header("Jump")]
    [SerializeField] float jumpForceOfPlayer = 1f;
    [SerializeField] float shortHopJumpPlayerMultiplier = 0.5f;
    [SerializeField] float normalGravity = 5f;
    [SerializeField] float fallingGravity = 6f;
    [SerializeField] float wallJumpForceX = 8f;
    [SerializeField] float wallJumpForceY = 8f;
    [SerializeField] float wallJumpControllDelayDuration = 0.2f;
    [SerializeField] int maxJumps = 1;
    [SerializeField] float maxFallSpeed = 12f;
    [Header("CoolDowns")]
    [SerializeField] float dashCoolDownDuration = 1f;
    [Header("LedgeGrab")]
    [SerializeField] float LedgeClimbDuration = 0.5f;
    [SerializeField] Vector2 ledgeGrabOffSet;
    [SerializeField] Vector2 climbOffSet = new Vector2(0.5f, 0.8f);
    int jumpRemaining;
    int dashsRemaining;
    float dashCoolDownTimer = 0f;
    Vector2 movement = new Vector2(0, 0);
    GroundDetector groundDetector;
    WallDetector wallDetector;
    Rigidbody2D myRigidBody2D;
    LedgeDetector ledgeDetector;

    enum MovementStates
    {
        Grounded,
        AirBorne,
        WallSliding,
        WallJumping,
        Dashing,
        LedgeGrabbing,
        LedgeHanging,
        LedgeClimbing,
        LedgeDropping
    }
    private MovementStates currentState = MovementStates.Grounded;

    private MovementStates ResolveGroundedState()
    {
        if (groundDetector.isGrounded)
            return MovementStates.Grounded;
        else if (wallDetector.GetWallDirection() != 0)
            return MovementStates.WallSliding;
        else
            return MovementStates.AirBorne;
    }

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
        if (dashCoolDownTimer > 0f) dashCoolDownTimer -= Time.deltaTime;
        MovementFunc();
        ApplyFallGravity();
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

    private bool MovementFuncGuard()
    {
        switch (currentState)
        {
            case MovementStates.WallJumping:
                return true;
            case MovementStates.Dashing:
                return true;
            case MovementStates.LedgeGrabbing:
                return true;
            case MovementStates.LedgeDropping:
                return true;
            case MovementStates.LedgeClimbing:
                return true;
            case MovementStates.LedgeHanging:
                return true;
            default:
                return false;
        }
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
        if (currentState == MovementStates.LedgeHanging)
        {
            if (input.y > 0 || movement.x * wallDetector.GetWallDirection() < 0f)
            {
                currentState = MovementStates.LedgeClimbing;
                Invoke(nameof(EndLedgeClimb), LedgeClimbDuration);
            }
            else if (input.y < 0)
            {
                currentState = MovementStates.LedgeDropping;
                myRigidBody2D.gravityScale = normalGravity;
                currentState = ResolveGroundedState();
            }
            return;
        }

    }
    // ledge grab
    private void GrabLedge()
    {
        currentState = MovementStates.LedgeGrabbing;
        myRigidBody2D.linearVelocity = Vector2.zero;
        myRigidBody2D.gravityScale = 0f;
        Vector2 pos = (Vector2)ledgeDetector.ledgeCheckPoint.position + ledgeGrabOffSet;
        transform.position = pos;
        currentState = MovementStates.LedgeHanging;
    }
    private void EndLedgeClimb()
    {
        transform.position += new Vector3(Mathf.Sign(transform.localScale.x) * climbOffSet.x, climbOffSet.y, 0);
        myRigidBody2D.gravityScale = normalGravity;
        currentState = ResolveGroundedState();
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
            Invoke(nameof(EndDash), dashDuration);
        }
    }
    private void EndDash()
    {
        myRigidBody2D.gravityScale = normalGravity;
        currentState = ResolveGroundedState();
    }
    // jumping
    public void OnJump(InputAction.CallbackContext context)
    {
        if (currentState == MovementStates.LedgeHanging && context.performed)
        {
            currentState = MovementStates.LedgeClimbing;
            Invoke(nameof(EndLedgeClimb), LedgeClimbDuration);
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
        }
        else if (wallDir != 0)
        {
            WallJump(wallDir);
        }
        else
        {
            myRigidBody2D.linearVelocityY = jumpForceOfPlayer;
        }
        jumpRemaining--;
    }

    // jumping - wall jump
    private void WallJump(float direction)
    {
        currentState = MovementStates.WallJumping;
        myRigidBody2D.linearVelocityX = direction * wallJumpForceX;
        transform.localScale = new Vector3(Mathf.Sign(direction), transform.localScale.y, transform.localScale.z);
        myRigidBody2D.linearVelocityY = wallJumpForceY;
        Invoke(nameof(EndWallJump), wallJumpControllDelayDuration);
    }

    private void EndWallJump()
    {
        currentState = ResolveGroundedState();
    }
}
