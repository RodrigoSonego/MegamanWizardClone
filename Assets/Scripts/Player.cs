using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private LayerMask platformLayers;

    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;

    private float moveX;
    private bool jumpPressed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) { jumpPressed = true; }
    }

    void FixedUpdate()
    {
        CheckJump();
        ApplyGravity();
        MovePlayer();
    }

    void MovePlayer()
    {
        if (moveX != 0)
        {
            //rb.MovePosition(rb.position + new Vector2(moveX ) * speed * Time.deltaTime);
            rb.velocity = new Vector2(speed * moveX, rb.velocity.y);
        }
    }

    void CheckJump()
    {
        if (jumpPressed && IsGrounded())
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpPressed = false;
        }
    }

    bool IsGrounded()
    {
        Vector2 boxCenter = rb.position - new Vector2(0f, 0.1f);
        Collider2D col = Physics2D.OverlapBox(boxCenter,
                                    playerCollider.bounds.size, 0f, platformLayers);
        return col != null;
    }

    void ApplyGravity()
    {
        if(rb.velocity.y < 0) { rb.gravityScale = fallMultiplier; }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump")) { rb.gravityScale = fallMultiplier; }
        else { rb.gravityScale = 1f; }

        if(moveX == 0f) { rb.velocity = new Vector2(0, rb.velocity.y); }
    }
}
