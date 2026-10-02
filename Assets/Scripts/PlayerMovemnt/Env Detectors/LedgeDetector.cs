using UnityEngine;

public class LedgeDetector : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform wallCheckPoint;
    [SerializeField] public Transform ledgeCheckPoint;
    WallDetector wallDetector;
    [SerializeField] float checkDistance = 0.64f;
    void Awake()
    {
        wallDetector = GetComponentInChildren<WallDetector>();
    }
    public bool IsLedgeAvailable()
    {
        float direction = -wallDetector.GetWallDirection();
        if (direction == 0) return false;
        Vector2 castDir = new Vector2(direction, 0);

        bool wallHit = Physics2D.Raycast(wallCheckPoint.position, castDir, checkDistance, groundLayer);
        bool ledgeHit = Physics2D.Raycast(ledgeCheckPoint.position, castDir, checkDistance, groundLayer);


        Debug.DrawRay(wallCheckPoint.position, castDir * checkDistance, wallHit ? Color.green : Color.red);
        Debug.DrawRay(ledgeCheckPoint.position, castDir * checkDistance, ledgeHit ? Color.green : Color.red);
        return wallHit && !ledgeHit;
    }
}
