using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerFade playerFade;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private Rigidbody2D myRigidBody;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDur = 0.5f;
    [SerializeField] private float hitStopDuration = 0.1f;
    [SerializeField] private CinemachineImpulseSource damageImpulse;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private float blinkInterval = 0.08f;
    public int currentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsInvincible;

    public int MaxHealth => maxHealth;
    public event Action<int, int> OnHealthChanged;   // current, max
    void Awake()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(int amount)
    {
        if (IsInvincible || IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        damageImpulse.GenerateImpulse();
        HitStopManager.Instance?.Freeze(hitStopDuration);
        StartInvincibility();
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        IsDead = true;
        CancelInvoke(nameof(EndInvincibility));   // <- kill the pending invoke
        if (blinkRoutine != null) { StopCoroutine(blinkRoutine); blinkRoutine = null; }
        playerSprite.enabled = true;
        IsInvincible = false;                      // <- reset the flag
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        playerMovement.enabled = false;
        myRigidBody.linearVelocity = Vector2.zero;
        myRigidBody.bodyType = RigidbodyType2D.Kinematic;
        yield return StartCoroutine(playerFade.FadeOut());

        Vector3 pos = CheckPointManager.Instance.GetCheckPoint();
        myRigidBody.position = pos;
        transform.position = pos;
        Physics2D.SyncTransforms();
        playerMovement.ResetForRespawn();
        playerAttack.ResetAttack();

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        IsDead = false;
        EnemyResetManager.Instance.ResetAllEnemies();

        yield return StartCoroutine(playerFade.FadeIn());
        playerMovement.enabled = true;
    }

    private void StartInvincibility()
    {
        IsInvincible = true;
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkRoutine());
        Invoke(nameof(EndInvincibility), invincibilityDur);
    }

    private void EndInvincibility()
    {
        IsInvincible = false;
    }
    private IEnumerator BlinkRoutine()
    {
        while (IsInvincible)
        {
            playerSprite.enabled = !playerSprite.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
        playerSprite.enabled = true;
    }
    private Coroutine blinkRoutine;
}
