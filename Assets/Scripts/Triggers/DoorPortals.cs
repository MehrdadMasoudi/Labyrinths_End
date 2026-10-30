using System.Collections.Generic;
using UnityEngine;

public class DoorPortals : MonoBehaviour
{
    private HashSet<GameObject> portalObjs = new HashSet<GameObject>();
    [SerializeField] private Transform destination;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (portalObjs.Contains(other.gameObject)) return;
        if (destination.TryGetComponent(out DoorPortals destinationPortal))
        {
            destinationPortal.portalObjs.Add(other.gameObject);
        }
        other.transform.position = destination.position;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        portalObjs.Remove(collision.gameObject);
    }
}
