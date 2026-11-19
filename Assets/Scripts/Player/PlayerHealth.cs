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
    public event Action<int, int> OnHealthChanged;
    public bool CanBeHit => !IsInvincible && !IsDead;

    private Coroutine blinkRoutine;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsInvincible || IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (damageImpulse != null) damageImpulse.GenerateImpulse();
        if (HitStopManager.Instance != null) HitStopManager.Instance.Freeze(hitStopDuration);

        StartInvincibility();
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        IsDead = true;
        CancelInvoke(nameof(EndInvincibility));
        if (blinkRoutine != null) { StopCoroutine(blinkRoutine); blinkRoutine = null; }
        if (playerSprite != null) playerSprite.enabled = true;
        IsInvincible = false;
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (playerMovement != null) playerMovement.enabled = false;

        if (myRigidBody != null)
        {
            myRigidBody.linearVelocity = Vector2.zero;
            myRigidBody.bodyType = RigidbodyType2D.Kinematic;
        }

        if (playerFade != null) yield return StartCoroutine(playerFade.FadeOut());

        Vector3 pos = transform.position;
        if (CheckPointManager.Instance != null)
            pos = CheckPointManager.Instance.GetCheckPoint();

        if (myRigidBody != null) myRigidBody.position = pos;
        transform.position = pos;
        Physics2D.SyncTransforms();

        if (playerMovement != null) playerMovement.ResetForRespawn();
        if (playerAttack != null) playerAttack.ResetAttack();

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        IsDead = false;

        if (EnemyResetManager.Instance != null) EnemyResetManager.Instance.ResetAllEnemies();

        if (playerFade != null) yield return StartCoroutine(playerFade.FadeIn());
        if (playerMovement != null) playerMovement.enabled = true;
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
            if (playerSprite != null) playerSprite.enabled = !playerSprite.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
        if (playerSprite != null) playerSprite.enabled = true;
    }
}