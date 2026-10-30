using UnityEngine;
using System.Collections.Generic;


public class EnemyResetManager : MonoBehaviour
{
    public static EnemyResetManager Instance { get; private set; }
    private readonly List<EnemyHealth> enemies = new();

    private void Awake() => Instance = this;
    public void Register(EnemyHealth enemy) => enemies.Add(enemy);

    public void ResetAllEnemies()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null) enemy.ResetEnemy();
        }
    }
}