using System.Collections.Generic;
using UnityEngine;

public class DoorPortals : MonoBehaviour
{
    private HashSet<GameObject> portalObjs = new HashSet<GameObject>();
    [SerializeField] private Transform destination;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var rb = other.attachedRigidbody;
        if (rb == null || !rb.TryGetComponent<PlayerMovement>(out _)) return;   // player only
        if (portalObjs.Contains(rb.gameObject)) return;
        if (destination.TryGetComponent(out DoorPortals destinationPortal))
            destinationPortal.portalObjs.Add(rb.gameObject);

        rb.position = destination.position;
        rb.linearVelocity = Vector2.zero;        // if i want to keep momentum this shouldnt exist;
        Physics2D.SyncTransforms();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        var rb = collision.attachedRigidbody;
        if (rb != null) portalObjs.Remove(rb.gameObject);
    }
}
