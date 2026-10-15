using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        Idle,
        Chase
    }
    [SerializeField] Transform playerPos;
    [SerializeField] float detectionRange = 4f;
    [SerializeField] float chaseSpeed = 2f;
    [SerializeField] float stopDistance = 1f;

    private Rigidbody2D myRB2D;
    private State currentState = State.Idle;

    private void Awake()
    {
        myRB2D = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        float distance = Vector2.Distance(transform.position, playerPos.position);
        currentState = distance <= detectionRange ? State.Chase : State.Idle;

        switch (currentState)
        {
            case State.Idle:
                StopHorizontalMovement();
                break;
            case State.Chase:
                ChasePlayer();
                break;
        }
    }

    private void StopHorizontalMovement()
    {
        myRB2D.linearVelocityX = 0f;
    }

    private void ChasePlayer()
    {
        float horizontalDistance = playerPos.position.x - transform.position.x;
        if (Mathf.Abs(horizontalDistance) <= stopDistance)
        {
            StopHorizontalMovement();
            return;
        }
        float direction = MathF.Sign(horizontalDistance);
        myRB2D.linearVelocityX = direction * chaseSpeed;
    }
}
