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

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] swordSwingSounds; // Array for your 10 sounds

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

        PlaySwordSound(); // Play the sound!

        activeHitBoxObject = GetHitBoxFor(attack);
        activeHitBox = activeHitBoxObject.GetComponent<SwordHitBox>();

        activeHitBoxObject.SetActive(true);
        activeHitBox.StartAttack();

        Invoke(nameof(DiasbleHitBox), hitBoxActiveDuration);
        Invoke(nameof(EndAttacking), AttackDuration);
    }

    private void PlaySwordSound()
    {
        if (audioSource != null && swordSwingSounds.Length > 0)
        {
            // 1. Randomize the pitch (0.9x to 1.1x) to make it feel less repetitive
            audioSource.pitch = Random.Range(0.9f, 1.1f);

            // 2. Pick a random sound from the 10 slots
            int randomIndex = Random.Range(0, swordSwingSounds.Length);

            // 3. Play the sound
            audioSource.PlayOneShot(swordSwingSounds[randomIndex]);

            // 4. Reset the pitch back to normal immediately for other sounds
            audioSource.pitch = 1f;
        }
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