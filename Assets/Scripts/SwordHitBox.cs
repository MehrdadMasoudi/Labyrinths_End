using UnityEngine;

public class SwordHitBox : MonoBehaviour
{
    [SerializeField] private LayerMask hittableLayers;
    [SerializeField] private int damage = 1;
    private bool IsAttacking = false;
    private bool HasHitAttack;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsAttacking || HasHitAttack) return;
        if (((1 << collision.gameObject.layer) & hittableLayers) == 0) return;
        // checks with bit shift for any  hittable layers via finding any bits in common
        if (collision.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
            HasHitAttack = true;
        }
    }
    public void StartAttack()
    {
        IsAttacking = true;
        HasHitAttack = false;
    }
    public void EndAttack()
    {
        IsAttacking = false;
    }
}
