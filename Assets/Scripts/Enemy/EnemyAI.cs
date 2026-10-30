using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] Transform playerPos;
    [SerializeField] float detectionRange = 10f;
    [SerializeField] float maxHeightDifference = 2f;
    [SerializeField] float chaseSpeed = 2f;
    [SerializeField] float stopDistance = 1f;
    [SerializeField] bool spriteFacesRight = true;

    [Header("Edge / wall checks")]
    [SerializeField] Transform groundCheck; // empty child, in front of the feet
    [SerializeField] Transform wallCheck;   // empty child, in front at mid-body
    [SerializeField] float groundCheckDistance = 1f;
    [SerializeField] float wallCheckDistance = 0.3f;
    [SerializeField] LayerMask groundLayer;

    private Rigidbody2D rb;
    private float stunTimer;
    private float facing = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (playerPos == null)
        {
            var p = FindAnyObjectByType<PlayerHealth>();
            if (p != null) playerPos = p.transform;
        }
    }

    public void Knockback(Vector2 velocity, float stunTime)
    {
        rb.linearVelocity = velocity;
        stunTimer = stunTime;
    }

    void FixedUpdate()
    {
        if (stunTimer > 0f)
        {
            stunTimer -= Time.fixedDeltaTime;
            rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, 0f, 30f * Time.fixedDeltaTime);
            return;
        }
        if (playerPos == null) return;

        Vector2 toPlayer = playerPos.position - transform.position;
        bool inRange = toPlayer.magnitude <= detectionRange &&
                       Mathf.Abs(toPlayer.y) <= maxHeightDifference;

        if (!inRange || Mathf.Abs(toPlayer.x) <= stopDistance)
        {
            rb.linearVelocityX = 0f;
            return;
        }

        float dir = Mathf.Sign(toPlayer.x);
        Face(dir);

        bool groundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        bool wallAhead = Physics2D.Raycast(wallCheck.position, new Vector2(facing, 0f), wallCheckDistance, groundLayer);

        rb.linearVelocityX = (groundAhead && !wallAhead) ? dir * chaseSpeed : 0f;
    }

    private void Face(float dir)
    {
        if (dir == facing) return;
        facing = dir;
        float s = Mathf.Abs(transform.localScale.x) * (spriteFacesRight ? dir : -dir);
        transform.localScale = new Vector3(s, transform.localScale.y, transform.localScale.z);
    }
}