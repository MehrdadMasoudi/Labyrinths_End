using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    public bool isGrounded = false;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float CheckRadius = 0.1f;
    void Update()
    {
        CheckForGrounded();
    }

    private void CheckForGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(transform.position, CheckRadius, groundLayer) != null;
    }
}
