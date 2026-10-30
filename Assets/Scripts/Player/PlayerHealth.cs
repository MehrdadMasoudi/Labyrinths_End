using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerFade playerFade;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private Rigidbody2D myRigidBody;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDur = 0.5f;
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
        StartInvincibility();
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        IsDead = true;
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
        Invoke(nameof(EndInvincibility), invincibilityDur);
    }

    private void EndInvincibility()
    {
        IsInvincible = false;
    }
}
