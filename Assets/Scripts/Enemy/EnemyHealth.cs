using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 2;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer sprite;
    [Tooltip("Optional. Auto-found in children if left empty.")]
    [SerializeField] private Animator animator;

    [Header("Death")]
    [Tooltip("Animator trigger fired on death. Leave empty to skip the animator path.")]
    [SerializeField] private string deathTriggerName = "Die";
    [Tooltip("Fallback fade time when no Animator/clip is available. " +
             "When a death clip exists, its length is used instead.")]
    [SerializeField] private float deathFadeTime = 1.0f;

    [Header("Hit Flash")]
    [SerializeField] private Color hitFlashColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.1f;
    [Tooltip("If true, the Animator owns sprite colour and the manual flash is skipped. " +
             "Enable when your Animator has a Hit state/clip that animates colour.")]
    [SerializeField] private bool animatorOwnsColor = false;

    public int currentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead { get; private set; }
    public Vector3 spawnPos;

    private Rigidbody2D rb;
    private IEnemyBrain brain;
    private TouchDamage[] touches;
    private Color baseColor;

    void Awake()
    {
        currentHealth = maxHealth;
        spawnPos = transform.position;
        rb = GetComponent<Rigidbody2D>();

        brain = GetComponent<IEnemyBrain>();
        touches = GetComponentsInChildren<TouchDamage>();

        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (sprite != null) baseColor = sprite.color;
    }

    void Start() => EnemyResetManager.Instance.Register(this);

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            // If the Animator drives colour, don't fight it with a manual flash.
            if (animatorOwnsColor && animator != null)
                animator.SetTrigger("Hit");
            else
                StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        if (sprite == null) yield break;
        sprite.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        if (!IsDead && sprite != null) sprite.color = baseColor;
    }

    private void Die()
    {
        IsDead = true;
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        if (brain != null) brain.DisableBrain();

        foreach (var t in touches) t.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // --- Animator-driven death ---
        if (animator != null && !string.IsNullOrEmpty(deathTriggerName))
        {
            animator.ResetTrigger(deathTriggerName);
            animator.SetTrigger(deathTriggerName);

            // One frame so the trigger is consumed and the state transition begins.
            yield return null;

            float clipLength = GetCurrentClipLength();
            float wait = clipLength > 0f ? clipLength : deathFadeTime;

            // Don't let a misconfigured clip length trap the object forever.
            if (wait <= 0f) wait = deathFadeTime;

            yield return new WaitForSeconds(wait);
        }
        // --- Fallback: manual alpha fade ---
        else if (sprite != null)
        {
            Color startColor = sprite.color;
            float t0 = 0f;
            while (t0 < deathFadeTime)
            {
                t0 += Time.deltaTime;
                var c = startColor;
                c.a = 1f - t0 / deathFadeTime;
                sprite.color = c;
                yield return null;
            }
        }

        gameObject.SetActive(false);
    }

    private float GetCurrentClipLength()
    {
        if (animator == null) return 0f;

        var info = animator.GetCurrentAnimatorClipInfo(0);
        if (info == null || info.Length == 0) return 0f;

        float len = info[0].clip != null ? info[0].clip.length : 0f;

        // If the clip is sped up by the Animator speed, account for it.
        float speed = Mathf.Abs(animator.speed);
        if (speed > 0.0001f) len /= speed;

        return len;
    }

    public void ResetEnemy()
    {
        StopAllCoroutines();

        currentHealth = maxHealth;
        IsDead = false;
        transform.position = spawnPos;

        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;

        // Reset Animator so the next life starts fresh, not stuck in "Die".
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            if (!string.IsNullOrEmpty(deathTriggerName))
                animator.ResetTrigger(deathTriggerName);
        }

        if (sprite != null) sprite.color = baseColor;

        if (brain != null) brain.EnableBrain();

        var minotaur = brain as Minotaur;
        if (minotaur != null) minotaur.GoToSleep();

        foreach (var t in touches) t.enabled = true;

        gameObject.SetActive(true);
    }
}