using UnityEngine;
using MovementStates = PlayerMovement.MovementStates;

// Everything that talks to the Animator lives here.
// It only READS from PlayerMovement and never changes movement.
[DefaultExecutionOrder(10)] // run after PlayerMovement.Update, like UpdateAnimator() used to
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerAttack playerAttack;
    [Header("Landing")]
    [SerializeField] float hardLand = 15f;
    [SerializeField] float softLand = 3f;
    [Header("Apex")]
    [SerializeField] float apexClipDuration = 0.625f;

    private bool wasRisingLastFrame;
    private bool isInApex = false;
    private MovementStates lastAnimState;
    PlayerMovement playerMovement;
    Rigidbody2D myRigidBody2D;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        myRigidBody2D = GetComponent<Rigidbody2D>();
    }
    void OnEnable()
    {
        playerMovement.LedgeClimbStarted += OnLedgeClimbStarted;
        playerMovement.LedgeClimbEnded += OnLedgeClimbEnded;

        playerMovement.Landed += OnLanded;
    }
    void OnDisable()
    {
        playerMovement.LedgeClimbStarted -= OnLedgeClimbStarted;
        playerMovement.LedgeClimbEnded -= OnLedgeClimbEnded;

        playerMovement.Landed -= OnLanded;
    }
    void Update()
    {
        CheckApex();
        UpdateAnimator();
    }

    private void OnLedgeClimbStarted() => animator.SetTrigger("StartLedgeClimb");
    private void OnLedgeClimbEnded() => animator.ResetTrigger("StartLedgeClimb");

    private void UpdateAnimator()
    {
        MovementStates currentState = playerMovement.CurrentState;
        //attack
        animator.SetBool("IsAttacking", playerAttack.IsAttacking);
        //
        float myRbFallSpeed = myRigidBody2D.linearVelocityY;
        ContiniousParamsAnimator(myRbFallSpeed, currentState);
        int id = currentState switch
        {
            MovementStates.Grounded => playerMovement.MoveInputX != 0 ? 1 : 0,
            MovementStates.AirBorne => myRbFallSpeed > -1 ? 2 : 3,
            MovementStates.KnockedBack => 3,
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

    private void ContiniousParamsAnimator(float rbFallSpeed, MovementStates currentState)
    {
        animator.SetFloat("VelocityY", rbFallSpeed);
        bool inApex = isInApex;
        animator.SetBool("InApex", inApex);
        bool inLand = animator.GetCurrentAnimatorStateInfo(0).IsName("Land") || animator.GetCurrentAnimatorStateInfo(0).IsName("LandFromDistance");
        animator.SetBool("InLand", inLand);
        bool inClimbing = currentState == MovementStates.LedgeClimbing;
        animator.SetBool("InClimbing", inClimbing);
    }
    private void CheckApex()
    {
        if (playerMovement.CurrentState != MovementStates.AirBorne)
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
    private void OnLanded(float fallDistance)
    {
        if (fallDistance > hardLand) animator.SetTrigger("EnterLandFromDistance");
        else if (fallDistance > softLand) animator.SetTrigger("EnterLand");
    }
}