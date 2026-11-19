using NUnit.Framework;
using UnityEngine;

public class CoreMechanicsTests
{
    [Test]
    public void AbilityUnlock_Dash_IncreasesMaxDashs()
    {
        GameObject playerGO = new GameObject();
        PlayerMovement player = playerGO.AddComponent<PlayerMovement>();
        InvokePrivateMethod(player, "Awake");

        Assert.AreEqual(0, player.MaxDashs);
        player.UnlockAbility(AbilityType.Dash);
        Assert.AreEqual(1, player.MaxDashs);

        Object.DestroyImmediate(playerGO);
    }

    [Test]
    public void Minotaur_EntersRageMode_WhenHealthDropsBelow70Percent()
    {
        GameObject playerGO = new GameObject("TestPlayer");
        playerGO.transform.position = new Vector3(10, 0, 0);

        GameObject bossGO = new GameObject();
        Rigidbody2D rb = bossGO.AddComponent<Rigidbody2D>();
        EnemyHealth health = bossGO.AddComponent<EnemyHealth>();
        Minotaur boss = bossGO.AddComponent<Minotaur>();

        SetPrivateField(boss, "rb", rb);
        SetPrivateField(boss, "player", playerGO.transform);
        SetPrivateField(boss, "health", health);
        SetPrivateField(health, "maxHealth", 20); // because mintoaur has 20 heart health

        InvokePrivateMethod(health, "Awake");
        InvokePrivateMethod(boss, "Start");

        // damage to reach 70%
        health.TakeDamage(6);
        InvokePrivateMethod(boss, "Update");
        Assert.IsFalse(boss.IsRaging, "Boss should NOT rage at exactly 70% in health.");

        // last damage to reach rage
        health.TakeDamage(1);
        InvokePrivateMethod(boss, "Update");
        Assert.IsTrue(boss.IsRaging, "Boss should rage when it is below 70% in health.");

        Object.DestroyImmediate(bossGO);
        Object.DestroyImmediate(playerGO);
    }
    [Test]
    public void Player_PogoBounce_SetsPositiveVerticalVelocity()
    {
        GameObject playerGO = new GameObject();
        Rigidbody2D rb = playerGO.AddComponent<Rigidbody2D>();
        PlayerMovement player = playerGO.AddComponent<PlayerMovement>();

        SetPrivateField(player, "myRigidBody2D", rb);
        rb.linearVelocity = new Vector2(0, -10f);

        player.PogoBounce();

        Assert.Greater(rb.linearVelocity.y, 0f);

        Object.DestroyImmediate(playerGO);
    }
    [Test]
    public void Enemy_LethalDamage_TriggersDeathState()
    {
        GameObject enemyGO = new GameObject();

        EnemyHealth enemyHealth = enemyGO.AddComponent<EnemyHealth>();
        enemyGO.AddComponent<Rigidbody2D>();

        InvokePrivateMethod(enemyHealth, "Awake");

        enemyHealth.TakeDamage(999);

        Assert.IsTrue(enemyHealth.IsDead, "enemy should be dead after 999 damage");

        Object.DestroyImmediate(enemyGO);
    }
    [Test]
    public void AbilityUnlock_DoubleJump_IncreasesMaxJumps()
    {
        GameObject playerGO = new GameObject();
        PlayerMovement player = playerGO.AddComponent<PlayerMovement>();
        InvokePrivateMethod(player, "Awake");

        int initialJumps = player.MaxJumps;

        player.UnlockAbility(AbilityType.DoubleJump);

        Assert.AreEqual(initialJumps + 1, player.MaxJumps, "Double Jump did not increase max jumps.");

        Object.DestroyImmediate(playerGO);
    }
    [Test]
    public void EnemyResetManager_RevivesDeadEnemies()
    {
        GameObject managerGO = new GameObject("ResetManager");
        EnemyResetManager manager = managerGO.AddComponent<EnemyResetManager>();
        InvokePrivateMethod(manager, "Awake");

        GameObject enemyGO = new GameObject("TestEnemy");
        enemyGO.AddComponent<Rigidbody2D>();
        EnemyHealth enemy = enemyGO.AddComponent<EnemyHealth>();
        InvokePrivateMethod(enemy, "Awake");
        InvokePrivateMethod(enemy, "Start");

        // killing the enemy
        enemy.TakeDamage(999);
        Assert.IsTrue(enemy.IsDead, "Enemy should be dead before reset.");

        // reseting all enemies to mock players death
        manager.ResetAllEnemies();

        // the enemy should be alive and at full health
        Assert.IsFalse(enemy.IsDead, "Enemy failed to revive");
        Assert.AreEqual(enemy.MaxHealth, enemy.currentHealth, "Enemy health did not refill after reseting");

        Object.DestroyImmediate(managerGO);
        Object.DestroyImmediate(enemyGO);
    }
    private void InvokePrivateMethod(object obj, string methodName)
    {
        var method = obj.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method != null) method.Invoke(obj, null);
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(obj, value);
    }
}