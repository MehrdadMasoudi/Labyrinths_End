using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    [SerializeField] private AbilityType ability;   // shows as a dropdown
    [SerializeField] private LayerMask playerLayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        var player = other.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        player.UnlockAbility(ability);
        Destroy(gameObject);
    }
}