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

    private float moveX;
    private bool jumpPressed;
    private float holdTime;

    private bool isShootCharged = false;
    private int shootCharge = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) { jumpPressed = true; }

        HandleShootPressing();
    }

    void FixedUpdate()
    {
        CheckJump();
        ApplyGravity();
        MovePlayer();
        CheckChargeShot();
    }

    void MovePlayer()
    {
        if (moveX != 0)
        {
            rb.velocity = new Vector2(speed * moveX, rb.velocity.y);

            shootSpawn.localPosition = new Vector3(0.5f * moveX, 0, 0);
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
        projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(shootSpawn.localPosition.x*2 * 7, 0); //PROVISÓRIO PELO AMOR DE DEUS MUDA ISSO
        //////////////////////////////////////////////////////////////////////////////////////////////////////QUANDO CHEGAR ANIMATOR AAAAAAAAAAAAAAAAAAAAAAAAAAAAA
        Destroy(projectile, 3f);       
    }

    void CheckChargeShot()
    {
        if (Input.GetButton("Fire1")) { holdTime += Time.fixedDeltaTime; }
        

        if (holdTime > 1f && holdTime < 2 ) { shootCharge = 1; isShootCharged = true; print("carregou 1"); }
        else if (holdTime >= 2f) { shootCharge = 2; print("carregou 2"); }
    }

}
