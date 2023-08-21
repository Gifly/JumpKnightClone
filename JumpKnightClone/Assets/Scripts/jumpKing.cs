using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class jumpKing : MonoBehaviour
{
    public float walkSpeed;
    public float moveInput;
    public bool isGrounded;
    public bool isTouchingWall;
    public Rigidbody2D rb;
    public LayerMask groundMask, wallMask;

    public PhysicsMaterial2D bounceMat, normalMat;
    public bool canJump = true;
    public bool canDoubleJump = false;
    public float jumpValue = 0.0f;

    public float checkpointThreshold = 100.0f;
    private float jumpDistance = 0.0f;
    private bool canSetCheckpoint = false;
    private Vector2 checkpointPosition;
    private bool hasCheckpoint = false;

    public Vector2 wallJumpForce;
    public float wallJumpTime = 0.5f;
    private float wallJumpTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        isGrounded = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y - 0.5f), 
            new Vector2(0.9f, 0.4f), 0f, groundMask);
        isTouchingWall = Physics2D.Raycast(transform.position, Vector2.right * Mathf.Sign(moveInput), 0.7f, wallMask);
        //Debug.Log(rb.velocity.y);

        if (isTouchingWall)
        {
            Debug.Log("isTouchingWall");
        }
        //if (isGrounded)
        //{
        //    Debug.Log("isTouchingGround");
        //}
        if (jumpValue == 0.0f && isGrounded)
        {
            rb.velocity = new Vector2(moveInput * walkSpeed, rb.velocity.y);
        }

       

        if (jumpValue > 0)
        {
            rb.sharedMaterial = bounceMat;
        }
        else
        {
            rb.sharedMaterial = normalMat;
        }

        if (Input.GetKey("space") && isGrounded && canJump)
        {
            jumpValue += 0.1f;
        }
        
        if(Input.GetKeyDown("space") && isGrounded && canJump)
        {
            rb.velocity = new Vector2(0.0f, rb.velocity.y);

        }

        if (Input.GetKeyDown("space") && canDoubleJump && !isGrounded)
        {
            Debug.Log("Double Jump");
            //Invoke("ResetJump", 0.2f);

            rb.velocity = new Vector2(rb.velocity.x, 10.0f);
            canDoubleJump = false;
        }

        /* if(isTouchingWall && Input.GetKeyDown("space"))
         {
             rb.velocity = new Vector2(-Mathf.Sign(moveInput) * wallJumpForce.x, wallJumpForce.y);
             wallJumpTimer = wallJumpTime;
         }*/


        if (jumpValue >= 20f && isGrounded)
        {
            float tempx = moveInput * walkSpeed;
            float tempy = jumpValue;
            rb.velocity = new Vector2(tempx, tempy);
            Invoke("ResetJump", 0.2f);
            canDoubleJump = true;

        }

        if (Input.GetKeyUp("space"))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(moveInput * walkSpeed, jumpValue);
                jumpValue = 0.0f;
                canDoubleJump = true;
                
            }
            canJump = true;
        }

        // Checkpoint logic
        jumpDistance += Mathf.Abs(rb.velocity.y) * Time.deltaTime;

        if (!hasCheckpoint && jumpDistance >= checkpointThreshold)
        {
            canSetCheckpoint = true;
            //Debug.Log("Can Checkpoint");
        }

        if (Input.GetKeyDown(KeyCode.C) && canSetCheckpoint)
        {
            checkpointPosition = transform.position;
            canSetCheckpoint = false;
            hasCheckpoint = true;
            //Debug.Log("Checkpoint setted");

        }

        if (Input.GetKeyDown(KeyCode.T) && hasCheckpoint)
        {
            transform.position = checkpointPosition;
            jumpDistance = 0.0f;
            //Debug.Log("Going to checkpoint");
        }
    }

    void ResetJump()
    {
        canJump = false;
        jumpValue = 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector2(transform.position.x, transform.position.y - 0.5f), new Vector2(0.9f, 0.2f));
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "wallMask")
        {
            Debug.Log("pared");
            WallTimer();
        }
    }

    void WallTimer()
    {
        Debug.Log("WT");
        if (wallJumpTimer > 0)
        {
            wallJumpTimer -= Time.deltaTime;
            Debug.Log(wallJumpTimer);
            if (wallJumpTimer <= 0)
            {
                rb.velocity = new Vector2(-Mathf.Sign(moveInput) * wallJumpForce.x, wallJumpForce.y);
                wallJumpTimer = wallJumpTime;
                Debug.Log("WallJump mamalon");
            }
        }
        
    }
        //if (wallJumpTimer >= 0)
        //{
        //    //Activar booleano de wallJump
        //    Debug.Log("Timer XD");
        //    wallJumpTimer -= Time.deltaTime;
        //    if (wallJumpTimer >= 0 && wallJump)
        //    {
        //        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -Mathf.Infinity, 0));
        //    }
        //}
}
