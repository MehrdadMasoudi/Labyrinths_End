using UnityEngine;

// A low wave that slides along the ground, damages the player once, and disappears.
// With "Spread Both Ways" on, one spawned wave creates a mirrored twin, so the boss
// code doesn't need to change.
[RequireComponent(typeof(Rigidbody2D))]
public class Shockwave : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private bool spreadBothWays = true;

    [Header("Optional: destroy on obstacles")]
    [Tooltip("Leave on Nothing if your walls share the Ground layer, or the wave dies at spawn.")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float obstacleCheckDistance = 0.3f;

    private Rigidbody2D rb;
    private float direction = 1f;
    private bool initialized;
    private bool hasHit;

    // Called on the mirrored twin, or by the boss if you later want to set the direction yourself.
    public void Init(float dir)
    {
        direction = Mathf.Sign(dir);
        initialized = true;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (!initialized && spreadBothWays)
        {
            GameObject twin = Instantiate(gameObject, transform.position, transform.rotation);
            twin.GetComponent<Shockwave>().Init(-1f);
            direction = 1f;
        }

        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.flipX = direction < 0f;

        rb.linearVelocity = new Vector2(direction * speed, 0f);
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (obstacleLayer.value == 0) return;

        Vector2 origin = (Vector2)transform.position + Vector2.up * 0.25f;
        if (Physics2D.Raycast(origin, Vector2.right * direction, obstacleCheckDistance, obstacleLayer).collider != null)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) return;

        hasHit = true;

        // If your PlayerHealth method has a different name or parameter type, change this one line.
        playerHealth.TakeDamage(damage);

        Destroy(gameObject);
    }
}
