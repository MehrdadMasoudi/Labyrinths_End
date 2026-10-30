using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Animator animator;
    [SerializeField] private float hitBoxActiveDuration = 0.5f;
    [SerializeField] private GameObject NormSwordHitBox;
    [SerializeField] private GameObject UpSwordHitBox;
    [SerializeField] private GameObject DownSwordHitBox;

    [SerializeField] float AttackDuration = 1f;
    private SwordHitBox activeHitBox;
    private GameObject activeHitBoxObject;
    public bool IsAttacking { get; private set; }
    public enum AttackType
    {
        AttackOne,
        AttackTwo,
        JumpAttack,
        FallDownAttack,
        UpAttack,
        WideAttack
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!enabled || IsAttacking || playerMovement.IsOnLedge) return;

        AttackType choosenAttack = ChooseAttackType();
        StartAttacking(choosenAttack);
    }

    private void StartAttacking(AttackType attack)
    {
        IsAttacking = true;
        animator.SetInteger("AttackID", (int)attack);
        animator.SetTrigger("Attack");

        activeHitBoxObject = GetHitBoxFor(attack);
        activeHitBox = activeHitBoxObject.GetComponent<SwordHitBox>();

        activeHitBoxObject.SetActive(true);
        activeHitBox.StartAttack();

        Invoke(nameof(DiasbleHitBox), hitBoxActiveDuration);
        Invoke(nameof(EndAttacking), AttackDuration);
    }

    private void DiasbleHitBox()
    {
        activeHitBox.EndAttack();
        activeHitBoxObject.SetActive(false);
    }

    private AttackType ChooseAttackType()
    {
        float verticalInput = playerMovement.MoveInputY;
        bool isGrounded = playerMovement.CurrentStateIsGrounded;

        if (verticalInput > 0.5f) return isGrounded ? AttackType.UpAttack : AttackType.JumpAttack;
        if (verticalInput < -0.5f && !isGrounded) return AttackType.FallDownAttack;

        return Random.value < 0.5f ? AttackType.AttackOne : AttackType.AttackTwo;
    }
    private GameObject GetHitBoxFor(AttackType attack)
    {
        switch (attack)
        {
            case AttackType.UpAttack:
            case AttackType.JumpAttack:
                return UpSwordHitBox;
            case AttackType.FallDownAttack:
                return DownSwordHitBox;
            default:
                return NormSwordHitBox;
        }
    }

    private void EndAttacking()
    {
        IsAttacking = false;
    }

    public void ResetAttack()
    {
        CancelInvoke();
        IsAttacking = false;
        if (activeHitBox != null) activeHitBox.EndAttack();
        if (activeHitBoxObject != null) activeHitBoxObject.SetActive(false);
    }
}
