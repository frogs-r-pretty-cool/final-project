using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float spawnHeight = 1.5f; // Adjust to spawn slightly above the ground
    public float throwForce = 5f; // Modify based on how strong you want throws to be

    private Rigidbody2D rb;
    public BoxCollider2D BallCollider;
    private BoxCollider2D other;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.position = new Vector2(transform.position.x, spawnHeight);
    }

    public void Throw(Vector2 direction, float power)
    {
        rb.velocity = direction.normalized * power * throwForce;
    }
}
