using System.Collections.Generic;
using UnityEngine;

public class SwordHitBox : MonoBehaviour
{
    [SerializeField] private LayerMask hittableLayers;
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask pogoableLayers;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Vector2 enemyKnockback = new Vector2(6f, 3f);
    [SerializeField] private float enemyStunTime = 0.2f;
    private ContactFilter2D pogoableFilter;
    private Collider2D hitCollider;
    private ContactFilter2D filter;
    private readonly List<Collider2D> results = new();
    private readonly HashSet<IDamageable> alreadyHit = new();
    private bool isAttacking;
    private bool pogoed;

    void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = hittableLayers,
            useTriggers = true
        };
        pogoableFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = pogoableLayers,
            useTriggers = true
        };
    }

    public void StartAttack()
    {
        pogoed = false;
        isAttacking = true;
        alreadyHit.Clear();
        Physics2D.SyncTransforms(); // hitbox was just enabled/flipped
        CheckOverlaps();            // catches enemies already inside
    }

    public void EndAttack() => isAttacking = false;

    void FixedUpdate()
    {
        if (isAttacking) CheckOverlaps(); // catches enemies that walk in mid-swing
    }

    private void CheckOverlaps()
    {
        hitCollider.Overlap(filter, results);
        foreach (var col in results)
        {
            var target = col.GetComponentInParent<IDamageable>();
            if (target != null && alreadyHit.Add(target))
            {
                var ai = col.GetComponentInParent<EnemyAI>();
                if (ai != null)
                {
                    float dir = Mathf.Sign(ai.transform.position.x - playerMovement.transform.position.x);
                    ai.Knockback(new Vector2(dir * enemyKnockback.x, enemyKnockback.y), enemyStunTime);
                }
                target.TakeDamage(damage);
            }
        }
        hitCollider.Overlap(pogoableFilter, results);
        if (!pogoed && results.Count > 0)
        {
            pogoed = true;
            playerMovement.PogoBounce();
        }
    }
}