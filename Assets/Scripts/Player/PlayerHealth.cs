using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerFade playerFade;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDur = 0.5f;
    public int currentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsInvincible;

    void Awake()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(int amount)
    {
        Debug.Log($"invincible: {IsInvincible}");
        if (IsInvincible || IsDead) return;

        currentHealth -= amount;
        Debug.Log("taken damage");
        StartInvincibility();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        playerMovement.enabled = false;
        yield return StartCoroutine(playerFade.FadeOut());

        transform.position = CheckPointManager.Instance.GetCheckPoint();
        currentHealth = maxHealth;
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
