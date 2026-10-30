
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 2;
    public int currentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public Vector3 spawnPos;

    void Awake()
    {
        currentHealth = maxHealth;
        spawnPos = transform.position;
    }
    void Start()
    {
        EnemyResetManager.Instance.Register(this);
    }
    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    private void Die()
    {
        IsDead = true;
        gameObject.SetActive(false);
    }
    public void ResetEnemy()
    {
        currentHealth = maxHealth;
        IsDead = false;
        transform.position = spawnPos;
        gameObject.SetActive(true);
    }
}
