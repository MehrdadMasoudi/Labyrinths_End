using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDur = 0.5f;
    [SerializeField] private PlayerAttack playerAttack;
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
        Debug.Log("dead");
        //logic of death

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
