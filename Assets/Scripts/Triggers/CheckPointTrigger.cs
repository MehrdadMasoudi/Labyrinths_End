using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;
        CheckPointManager.Instance.SetCheckPoint(transform.position);
    }
}
