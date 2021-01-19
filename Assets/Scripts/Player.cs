using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask platformLayers;

    private Rigidbody2D rb;
    private float moveX;
    private bool jumpPressed;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();        
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) { jumpPressed = true; }
    }

    void FixedUpdate()
    {
        CheckJump();
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
        if (jumpPressed)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpPressed = false;
        }
    }
}
