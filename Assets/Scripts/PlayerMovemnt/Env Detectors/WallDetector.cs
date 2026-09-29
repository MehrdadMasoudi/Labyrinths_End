using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [SerializeField] LayerMask wallLayer;
    [SerializeField] float wallCheckDistance = 0.64f;
    public bool isTouchingWallRight;
    public bool isTouchingWallLeft;
    void Update()
    {
        CheckWalls();
    }
    void CheckWalls()
    {
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);

        isTouchingWallRight = hitRight.collider != null;
        isTouchingWallLeft = hitLeft.collider != null;

        Debug.DrawRay(transform.position, Vector2.right * wallCheckDistance, isTouchingWallRight ? Color.green : Color.red);
        Debug.DrawRay(transform.position, Vector2.left * wallCheckDistance, isTouchingWallLeft ? Color.green : Color.red);

    }
    public float GetWallDirection()
    {
        if (isTouchingWallRight) return -1;
        if (isTouchingWallLeft) return 1;
        return 0f;
    }
}
