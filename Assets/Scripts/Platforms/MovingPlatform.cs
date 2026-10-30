using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private LayerMask playerLayer;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 previousPosition;
    private readonly List<Rigidbody2D> playersOnPlatform = new List<Rigidbody2D>();

    private void Start()
    {
        startPoint = transform.position;
        endPoint = pointB.position;
        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        float t = Mathf.PingPong(Time.time * moveSpeed, 1f);
        transform.position = Vector3.Lerp(startPoint, endPoint, t);

        Vector3 delta = transform.position - previousPosition;
        foreach (var rb in playersOnPlatform)
        {
            rb.position += (Vector2)delta;
        }
        previousPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;

        var rb = collision.rigidbody;
        if (rb != null) playersOnPlatform.Add(rb);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;

        var rb = collision.rigidbody;
        if (rb != null) playersOnPlatform.Remove(rb);
    }
}