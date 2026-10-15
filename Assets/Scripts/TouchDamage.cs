using UnityEngine;

public class TouchDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }
    }
}
