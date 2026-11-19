using NUnit.Framework;
using UnityEngine;

public class CombatLogicTests
{
    [Test]
    public void Player_TakeDamage_ReducesHealth()
    {
        GameObject playerGO = new GameObject();
        PlayerHealth playerHealth = playerGO.AddComponent<PlayerHealth>();
        InvokePrivateMethod(playerHealth, "Awake");

        int initialHealth = playerHealth.currentHealth;
        playerHealth.TakeDamage(1);

        Assert.AreEqual(initialHealth - 1, playerHealth.currentHealth);

        Object.DestroyImmediate(playerGO);
    }

    [Test]
    public void Player_IsInvincible_AfterTakingDamage()
    {
        GameObject playerGO = new GameObject();
        PlayerHealth playerHealth = playerGO.AddComponent<PlayerHealth>();
        InvokePrivateMethod(playerHealth, "Awake");

        playerHealth.TakeDamage(1);

        Assert.IsTrue(playerHealth.IsInvincible, "Player should be invincible immediately after taking damage.");

        Object.DestroyImmediate(playerGO);
    }

    [Test]
    public void Enemy_TakeDamage_ReducesHealth()
    {
        GameObject enemyGO = new GameObject();
        EnemyHealth enemyHealth = enemyGO.AddComponent<EnemyHealth>();
        InvokePrivateMethod(enemyHealth, "Awake");

        int initialHealth = enemyHealth.currentHealth; // 2
        enemyHealth.TakeDamage(1);

        Assert.AreEqual(initialHealth - 1, enemyHealth.currentHealth);

        Object.DestroyImmediate(enemyGO);
    }

    // --- HELPER METHOD for invoking private methods as name suggests ---
    private void InvokePrivateMethod(object obj, string methodName)
    {
        var method = obj.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (method != null)
            method.Invoke(obj, null);
    }
}