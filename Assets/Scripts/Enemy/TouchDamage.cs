using System.Collections.Generic;
using UnityEngine;

public class TouchDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool destroyOnHit = false; // tick for bullets
    [SerializeField] private bool knockbackPlayer = false;
    [SerializeField] private Vector2 knockbackForce = new Vector2(8f, 6f);
    [SerializeField] private float knockbackTime = 0.2f;
    private Collider2D triggerCollider;
    private ContactFilter2D filter;
    private readonly List<Collider2D> results = new();

    private void Awake()
    {
        foreach (var c in GetComponents<Collider2D>())
            if (c.isTrigger) { triggerCollider = c; break; }

        filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = targetLayers
        };
    }

    private void FixedUpdate()
    {
        if (triggerCollider == null) return;

        triggerCollider.Overlap(filter, results);
        foreach (var col in results)
        {
            if (col.GetComponentInParent<SwordHitBox>() != null) continue;

            var target = col.GetComponentInParent<IDamageable>();
            if (target == null) continue;

            var health = target as PlayerHealth;
            bool willHurt = health != null && !health.IsInvincible && !health.IsDead;
            target.TakeDamage(damage);
            if (knockbackPlayer && willHurt && !health.IsDead)
            {
                var pm = health.GetComponent<PlayerMovement>();
                float dir = Mathf.Sign(health.transform.position.x - transform.position.x);
                pm.Knockback(new Vector2(dir * knockbackForce.x, knockbackForce.y), knockbackTime);
            }
            if (destroyOnHit) { Destroy(gameObject); return; }
        }
    }
}