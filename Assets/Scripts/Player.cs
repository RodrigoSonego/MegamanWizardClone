using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    [Header("Moving")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float fallMultiplier;
    [Header("Sliding")]
    [SerializeField] private float slideTime;
    [SerializeField] private float slideForce;
    [Header("Shooting")]
    [SerializeField] private GameObject[] shootPrefabs;
    [SerializeField] private Transform shootSpawn;
    [SerializeField] private float shootAnimationTime;
    [Space]
    [SerializeField] private LayerMask platformLayers;

    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private int directionFacing = 1;
    private bool isFacingRight = true;
    
    private float moveX;
    private bool jumpPressed;
    private float holdTime;

    private bool isShootCharged = false;
    private int shootCharge = 0;
    private float shootTimer;

    private bool isWallSliding = false;
    private bool isWallJumping = false;

    private bool isSliding = false;
    private bool downPressed = false;

    private Coroutine shootingAnimation;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandleMovementInput();
        HandleShootPressing();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        CheckWallSlide();
        CheckSlide();
        CheckJump();
        MovePlayer();
        ApplyGravity();
     
        CheckChargeShot();
    }

    void MovePlayer()
    {
        if (moveX != 0 && isWallJumping == false && isSliding == false)
        {           
            rb.velocity = new Vector2(speed * moveX, rb.velocity.y);
            if(!isWallSliding)directionFacing = (int)moveX;
        }      
    }

    void CheckJump()
    {
        if(jumpPressed == false || isSliding) { return; }
        else if (IsGrounded())
        {
            Jump();
        } 
        else if (IsTouchingWall())
        {
            WallJump();
        }
        //else if (isSliding) 
        //{
        //    SlideJump();
        //}
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpPressed = false;
    }

    void WallJump()
    {
        isWallJumping = true;

        if (isWallSliding)
        {
            rb.velocity = new Vector2(-directionFacing * 4, jumpForce);
        } 
        else
        {
            rb.velocity = new Vector2(-directionFacing * 4, jumpForce);
            directionFacing = -directionFacing;
        }

        jumpPressed = false;
        StartCoroutine(WallJumpCooldown());
    }

    void SlideJump()
    {
        rb.velocity = new Vector2(slideForce / 1.2f, jumpForce);
        jumpPressed = false;
        isSliding = false;
        StopCoroutine("SlideTime");
    }

    IEnumerator WallJumpCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        isWallJumping = false;
    }

    bool IsGrounded()
    {
        Vector2 boxCenter = rb.position - new Vector2(0f, 0.2f);
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

        if (moveX == 0f && rb.velocity.y != 0) { rb.velocity = new Vector2(0, rb.velocity.y); }
    }

    void HandleMovementInput()
    {
        moveX = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump")) { jumpPressed = true; }
        if(Input.GetButtonUp("Jump")) { jumpPressed = false; }

        if(Input.GetAxisRaw("Vertical") < 0) { downPressed = true; }
        else { downPressed = false; }
    }

    void HandleShootPressing()
    {
        if (Input.GetButtonDown("Fire1")) 
        {   Fire(shootCharge);
            animator.SetBool("shooting", true);
            shootTimer = 0;
            if (shootingAnimation != null) { StopCoroutine(shootingAnimation); }
        }

        if (Input.GetButtonUp("Fire1") && isShootCharged)
        {           
            Fire(shootCharge);
            holdTime = 0;
            shootCharge = 0;
            isShootCharged = false;
            shootingAnimation = StartCoroutine(StopShootingAnimation());
        }
        else if (Input.GetButtonUp("Fire1")) 
        { 
            holdTime = 0;
            shootingAnimation = StartCoroutine(StopShootingAnimation());
        }
    }

    IEnumerator StopShootingAnimation()
    {
        yield return new WaitForSeconds(shootAnimationTime);
        animator.SetBool("shooting", false);
    }

    void UpdateAnimations()
    {
        if(directionFacing == 1 && !isFacingRight) { isFacingRight = true; spriteRenderer.flipX = false; FlipShootSpawn(); }
        else if (directionFacing == -1 && isFacingRight) { isFacingRight = false; spriteRenderer.flipX = true; FlipShootSpawn(); }        

        if (IsGrounded() == false) { animator.SetBool("jumping", true); }
        else { animator.SetBool("jumping", false); }

        if(moveX != 0) { animator.SetBool("moving", true); }
        else { animator.SetBool("moving", false); }

        if (shootTimer >= shootAnimationTime) { animator.SetBool("shooting", false); }
        else if( isWallSliding) { StopCoroutine("StopShootingAnimation"); animator.SetBool("shooting", false); }
    }

    void FlipShootSpawn()
    {
        shootSpawn.localPosition = new Vector3(-shootSpawn.localPosition.x, shootSpawn.localPosition.y, 0);
    }

    void Fire(int shootIndex)
    {
        GameObject projectile = Instantiate(shootPrefabs[shootIndex], shootSpawn.transform.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(directionFacing * 7, 0);
        Destroy(projectile, 3f);       
    }



    void CheckChargeShot()
    {
        if (Input.GetButton("Fire1") == false) { return; }
        else { holdTime += Time.fixedDeltaTime;}
        

        if (holdTime > 1f && holdTime < 2 ) { shootCharge = 1; isShootCharged = true; }
        else if (holdTime >= 2f) { shootCharge = 2; }
    }

    bool IsTouchingWall()
    {
        Vector2 boxRight = rb.position + new Vector2(0.1f * directionFacing, 0f);
        Collider2D col = Physics2D.OverlapBox(boxRight,
                                    playerCollider.bounds.size - new Vector3(0,0.3f,0), 0f, platformLayers);

        Vector2 boxLeft = rb.position + new Vector2(0.1f * directionFacing, 0f);
        Collider2D leftcol = Physics2D.OverlapBox(boxLeft,
                                    playerCollider.bounds.size - new Vector3(0, 0.3f, 0), 0f, platformLayers);


        return col != null || leftcol != null;
    }

    void CheckWallSlide()
    {
        if(moveX == directionFacing && IsTouchingWall() && !IsGrounded())
        {
            isWallSliding = true;
            animator.SetBool("wallSliding", true);
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -1.5f, float.MaxValue));
        } 
        else
        {
            isWallSliding = false;
            animator.SetBool("wallSliding", false);
        }
    }

    void Slide()
    {
        isSliding = true;
        animator.SetBool("sliding", true);
        rb.velocity = new Vector2(slideForce * directionFacing, rb.velocity.y);
        playerCollider.sharedMaterial.friction = 0f;
        StartCoroutine(SlideTime(slideTime));
    }

    IEnumerator SlideTime(float time)
    {       
        yield return new WaitForSeconds(time);
        playerCollider.sharedMaterial.friction = 10;
        isSliding = false;
        animator.SetBool("sliding", false);
    }

    void CheckSlide()
    {
        if(downPressed && jumpPressed && !isSliding)
        {
            Slide();
        }

        if(isSliding && rb.velocity.x == 0)
        {
            isSliding = false;
            playerCollider.sharedMaterial.friction = 10;
            StopCoroutine("SlideTime");
        }
    }
 

    void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position - new Vector3(0,0.1f,0), new Vector2(0, -0.4f), Color.red) ;

        Debug.DrawRay(transform.position, new Vector2(0.3f, 0), Color.yellow);

        Debug.DrawRay(transform.position, new Vector2(-0.3f, 0), Color.yellow);

    }
}
