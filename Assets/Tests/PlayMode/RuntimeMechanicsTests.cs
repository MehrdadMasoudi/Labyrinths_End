using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RuntimeMechanicsTests
{
    [SetUp]
    public void ResetAllSingletons()
    {
        // Force-clear the static instances from previous tests
        var cpType = typeof(CheckPointManager);
        var cpField = cpType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
        if (cpField != null) cpField.SetValue(null, null);

        var ermType = typeof(EnemyResetManager);
        var ermField = ermType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
        if (ermField != null) ermField.SetValue(null, null);
    }

    [UnityTest]
    public IEnumerator Player_DiesAndRespawns_RegainsHealth()
    {
        GameObject cpManager = new GameObject("CheckPointManager");
        cpManager.AddComponent<CheckPointManager>();

        GameObject enemyManager = new GameObject("EnemyResetManager");
        enemyManager.AddComponent<EnemyResetManager>();

        GameObject playerGO = new GameObject("Player");
        playerGO.AddComponent<Rigidbody2D>();
        playerGO.AddComponent<SpriteRenderer>();

        var move = playerGO.AddComponent<PlayerMovement>();
        move.enabled = false;

        playerGO.AddComponent<PlayerAttack>();
        var fade = playerGO.AddComponent<PlayerFade>();
        fade.enabled = false;

        PlayerHealth health = playerGO.AddComponent<PlayerHealth>();

        yield return null;

        int startHealth = health.currentHealth;

        // Deal lethal damage
        health.TakeDamage(999);

        // Wait for the full DeathSequence to complete
        yield return new WaitForSeconds(1f);

        // The whole point of the test: Player should have survived death and be back with full health
        Assert.IsFalse(health.IsDead, "Player should have respawned.");
        Assert.AreEqual(startHealth, health.currentHealth, "Player should have full health after respawn.");

        Object.Destroy(playerGO);
        Object.Destroy(cpManager);
        Object.Destroy(enemyManager);
    }
    [UnityTest]
    public IEnumerator Player_TakesDamage_BecomesInvincibleForAWhile()
    {
        GameObject playerGO = new GameObject("Player");
        playerGO.AddComponent<Rigidbody2D>();
        playerGO.AddComponent<SpriteRenderer>();

        var move = playerGO.AddComponent<PlayerMovement>();
        move.enabled = false;

        playerGO.AddComponent<PlayerAttack>();
        var fade = playerGO.AddComponent<PlayerFade>();
        fade.enabled = false;

        PlayerHealth health = playerGO.AddComponent<PlayerHealth>();

        yield return null;

        int initialHealth = health.currentHealth;
        health.TakeDamage(1);

        Assert.AreEqual(initialHealth - 1, health.currentHealth);
        Assert.IsTrue(health.IsInvincible);

        yield return new WaitForSeconds(0.6f);

        Assert.IsFalse(health.IsInvincible);

        Object.Destroy(playerGO);
    }

    [UnityTest]
    public IEnumerator Enemy_TakesLethalDamage_DisablesPhysics()
    {
        // Setup Manager
        GameObject enemyManager = new GameObject("EnemyResetManager");
        enemyManager.AddComponent<EnemyResetManager>();

        GameObject enemyGO = new GameObject("Enemy");
        Rigidbody2D rb = enemyGO.AddComponent<Rigidbody2D>();
        enemyGO.AddComponent<SpriteRenderer>();
        EnemyHealth health = enemyGO.AddComponent<EnemyHealth>();

        yield return null;

        health.TakeDamage(999);

        Assert.IsFalse(rb.simulated, "Enemy Rigidbody should be disabled upon death.");
        Assert.IsTrue(health.IsDead, "Enemy should be marked as dead.");

        Object.Destroy(enemyGO);
        Object.Destroy(enemyManager);
    }

    private void InvokePrivateMethod(object obj, string methodName)
    {
        var method = obj.GetType().GetMethod(methodName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null) method.Invoke(obj, null);
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) field.SetValue(obj, value);
    }
}