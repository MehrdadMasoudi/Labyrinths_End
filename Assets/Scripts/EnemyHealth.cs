
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 2;
    public int currentHealth { get; private set; }
    public bool IsDead { get; private set; }

    void Awake()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        Debug.Log("I gpt hit");
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
        //logic of death
        Debug.Log("i died");
    }

}
