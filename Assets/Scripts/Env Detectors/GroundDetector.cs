using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    public bool isGrounded = false;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Vector2 checkSize = new Vector2(0.8f, 0.1f); // a bit narrower than your collider

    void Update()
    {
        isGrounded = Physics2D.OverlapBox(transform.position, checkSize, 0f, groundLayer) != null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, checkSize);
    }
}