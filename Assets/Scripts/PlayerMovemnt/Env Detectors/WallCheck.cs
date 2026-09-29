using UnityEngine;

public class WallCheck : MonoBehaviour
{
    public bool isTouchingWall = false;
    void OnTriggerEnter2D(Collider2D collision)
    {
        isTouchingWall = true;
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        isTouchingWall = false;
    }
}
