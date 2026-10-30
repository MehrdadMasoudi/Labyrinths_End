using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float deathFadeTime = 0.25f;

    public int currentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public Vector3 spawnPos;

    private Rigidbody2D rb;
    private EnemyAI ai;
    private TouchDamage[] touches;
    private Color baseColor;

    void Awake()
    {
        currentHealth = maxHealth;
        spawnPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        ai = GetComponent<EnemyAI>();
        touches = GetComponentsInChildren<TouchDamage>();
        baseColor = sprite.color;
    }
    void Start() => EnemyResetManager.Instance.Register(this);

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0) { currentHealth = 0; Die(); }
    }

    private void Die()
    {
        IsDead = true;
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        ai.enabled = false;
        foreach (var t in touches) t.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        float t0 = 0f;
        while (t0 < deathFadeTime)
        {
            t0 += Time.deltaTime;
            var c = baseColor; c.a = 1f - t0 / deathFadeTime;
            sprite.color = c;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    public void ResetEnemy()
    {
        StopAllCoroutines();
        currentHealth = maxHealth;
        IsDead = false;
        transform.position = spawnPos;
        sprite.color = baseColor;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        ai.enabled = true;
        foreach (var t in touches) t.enabled = true;
        gameObject.SetActive(true);
    }
}