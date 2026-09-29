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
    [Header("CoolDowns")]
    [SerializeField] float dashCoolDownDuration = 1f;
    int jumpRemaining;
    int dashsRemaining;
    float dashCoolDownTimer = 0f;
    bool isWallJumping = false;
    bool isDashing = false;
    Vector2 movement = new Vector2(0, 0);
    GroundDetector groundDetector;
    WallDetector wallDetector;
    Rigidbody2D myRigidBody2D;
    void Awake()
    {
        groundDetector = GetComponentInChildren<GroundDetector>();
        wallDetector = GetComponentInChildren<WallDetector>();
    }
    void Start()
    {
        myRigidBody2D = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (dashCoolDownTimer > 0f) dashCoolDownTimer -= Time.deltaTime;
        MovingFunc();
    }

    private void MovingFunc()
    {
        FlipSprite();
        if (groundDetector.isGrounded || wallDetector.isTouchingWallRight || wallDetector.isTouchingWallLeft)
        {
            jumpRemaining = maxJumps;
            dashsRemaining = maxDashs;
        }
        if (isWallJumping || isDashing) return;
        { myRigidBody2D.linearVelocityX = movement.x * speedOfPlayer; }
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
        movement.x = context.ReadValue<Vector2>().x;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        float direction = Mathf.Sign(transform.localScale.x);
        if (context.performed && dashCoolDownTimer <= 0f && dashsRemaining > 0)
        {
            dashCoolDownTimer = dashCoolDownDuration;
            dashsRemaining--;
            isDashing = true;
            myRigidBody2D.linearVelocityX = direction * dashSpeedOfPlayer;
            myRigidBody2D.linearVelocityY = 0f;
            myRigidBody2D.gravityScale = 0f;
            Invoke(nameof(EndDash), dashDuration);
        }
    }
    private void EndDash()
    {
        isDashing = false;
        myRigidBody2D.gravityScale = normalGravity;
    }
    // jumping
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && jumpRemaining > 0)
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
        isWallJumping = true;
        myRigidBody2D.linearVelocityX = direction * wallJumpForceX;
        FlipSprite();
        myRigidBody2D.linearVelocityY = wallJumpForceY;
        Invoke(nameof(EndWallJump), wallJumpControllDelayDuration);
    }

    private void EndWallJump()
    {
        isWallJumping = false;
    }
}
