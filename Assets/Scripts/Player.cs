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
    [Space]
    [SerializeField] private GameObject[] shootPrefabs;
    [SerializeField] private Transform shootSpawn;
    [Space]
    [SerializeField] private LayerMask platformLayers;

    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;

    private int directionFacing = 1; //se pa provisório, dependendo de como for feito o animator
    
    private float moveX;
    private bool jumpPressed;
    private float holdTime;

    private bool isShootCharged = false;
    private int shootCharge = 0;

    private bool isWallSliding = false;
    private bool isWallJumping = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        HandleMovementInput();
        HandleShootPressing();
    }

    void FixedUpdate()
    {
        MovePlayer();
        CheckWallSlide();
        CheckJump();
        ApplyGravity();
     
        CheckChargeShot();
    }

    void MovePlayer()
    {
        if (moveX != 0 && isWallJumping == false)
        {
            float actualSpeed = speed;
            if(!IsGrounded()) { actualSpeed = speed / 1.3f; }
            rb.velocity = new Vector2(actualSpeed * moveX, rb.velocity.y);
            directionFacing = (int)moveX;

            shootSpawn.localPosition = new Vector3(0.5f * moveX, 0, 0);
        }
    }

    void CheckJump()
    {
        if (jumpPressed && IsGrounded())
        {
            Jump();
        } 
        else if (jumpPressed && isWallSliding)
        {
            WallJump();
        }
    }

    void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpPressed = false;
    }

    void WallJump()
    {
        isWallJumping = true;
        rb.AddForce(new Vector2(-moveX * 10, jumpForce), ForceMode2D.Impulse);
        jumpPressed = false;
        StartCoroutine(WallJumpCooldown());
    }

    IEnumerator WallJumpCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        isWallJumping = false;
    }

    bool IsGrounded()
    {
        Vector2 boxCenter = rb.position - new Vector2(0f, 0.1f);
        Collider2D col = Physics2D.OverlapBox(boxCenter,
                                    playerCollider.bounds.size - new Vector3(0.1f,0,0), 0f, platformLayers);
        return col != null;
    }

    void ApplyGravity()
    {
        if(isWallSliding || isWallJumping) { rb.gravityScale = 1f; return;  }

        if(rb.velocity.y < 0) { rb.gravityScale = fallMultiplier; }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump")) { rb.gravityScale = fallMultiplier; }
        else { rb.gravityScale = 1f; }

        if(moveX == 0f) { rb.velocity = new Vector2(0, rb.velocity.y); }
    }

    void HandleMovementInput()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) { jumpPressed = true; }
    }

    void HandleShootPressing()
    {
        if (Input.GetButtonDown("Fire1")) { Fire(shootCharge); }

        if (Input.GetButtonUp("Fire1") && isShootCharged)
        {
            Fire(shootCharge);
            holdTime = 0;
            shootCharge = 0;
            isShootCharged = false;
        }
        else if (Input.GetButtonUp("Fire1")) { holdTime = 0; }
    }


    void Fire(int shootIndex)
    {
        GameObject projectile = Instantiate(shootPrefabs[shootIndex], shootSpawn.transform.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(directionFacing * 7, 0);
        Destroy(projectile, 3f);       
    }


    void CheckChargeShot()
    {
        if (Input.GetButton("Fire1")) { holdTime += Time.fixedDeltaTime; }
        

        if (holdTime > 1f && holdTime < 2 ) { shootCharge = 1; isShootCharged = true; print("carregou 1"); }
        else if (holdTime >= 2f) { shootCharge = 2; print("carregou 2"); }
    }

    bool IsTouchingWall()
    {
        Vector2 boxCenter = rb.position + new Vector2(0.1f * directionFacing, 0f);
        Collider2D col = Physics2D.OverlapBox(boxCenter,
                                    playerCollider.bounds.size - new Vector3(0,0.3f,0), 0f, platformLayers);

        return col != null;
    }

    void CheckWallSlide()
    {
        if(moveX == directionFacing && IsTouchingWall() && !IsGrounded())
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -1.5f, float.MaxValue));
        } 
        else
        {
            isWallSliding = false;
        }
    }

}
