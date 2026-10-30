using System.Collections.Generic;
using UnityEngine;

public class TouchDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool destroyOnHit = false; // tick for bullets

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
            var target = col.GetComponentInParent<IDamageable>();
            if (target == null) continue;

            target.TakeDamage(damage);
            if (destroyOnHit) { Destroy(gameObject); return; }
        }
    }
}