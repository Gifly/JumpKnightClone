using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class jumpKing : MonoBehaviour
{
    public Rigidbody2D rb;
    public float walkSpeed;
    public float moveInput;
    public LayerMask groundMask, wallMask;
    private bool isFacingRight = true;

    public PhysicsMaterial2D bounceMat, normalMat;
    public bool canJump = true;
    public bool canDoubleJump = false;
    public float jumpValue = 0.0f;
    private float jumpDistance = 0.0f;

    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    public Vector2 wallJumpingPower = new Vector2(2f, 16f);

    public float checkpointThreshold = 10.0f;
    private bool canSetCheckpoint = false;
    private Vector2 checkpointPosition;
    private bool hasCheckpoint = false;

    public Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        //Movement Controller
        moveInput = Input.GetAxisRaw("Horizontal");

        Flip();
        //animation controller
        if(Mathf.Abs(moveInput) > 0 && rb.velocity.y == 0)
        {
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }

        if(rb.velocity.y == 0)
        {
            anim.SetBool("isJumping", false);
            anim.SetBool("isFalling", false);
        }

        if(rb.velocity.y > 0)
        {
            anim.SetBool("isJumping", true);
        }

        if (rb.velocity.y < 0)
        {
            anim.SetBool("isFalling", true);
            anim.SetBool("isJumping", false);
        }

        if (isGrounded())
        {
            canJump = true;
        }
        //Move when it is not walking and is touching ground
        if (jumpValue == 0.0f && isGrounded())
        {
            rb.velocity = new Vector2(moveInput * walkSpeed, rb.velocity.y);
        }

        //Change physics materials if is jumping or not for bouncingness
        //if (jumpValue > 0)
        //{
        //    rb.sharedMaterial = bounceMat;
        //}
        //else
        //{
        //    rb.sharedMaterial = normalMat;
        //}

        //When click space and is touching ground, start charging the jump force
        if (Input.GetKey("space") && isGrounded() && canJump)
        {
            jumpValue += 0.1f;
            anim.SetBool("chargingJump", true);
        }
        //In the instant that space is pressed and is grounded, stop moving and mantain y movemement.
        if(Input.GetKeyDown("space") && isGrounded() && canJump)
        {
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }
        //If is not touching ground and can double jump, when press space, add force in y
        if (Input.GetKeyDown("space") && canDoubleJump && !isGrounded() && !isWalled())
        {
            Debug.Log("Double Jump");
            //Invoke("ResetJump", 0.2f);
            rb.velocity = new Vector2(rb.velocity.x, 10.0f);
            canDoubleJump = false;
        }
        //If jump value is fully charged and it is touching the ground, jump
        if (jumpValue >= 20f && isGrounded())
        {
            float tempx = moveInput * walkSpeed;
            float tempy = jumpValue;
            rb.velocity = new Vector2(tempx, tempy);
            Invoke("ResetJump", 0.2f);
            canDoubleJump = true;
            anim.SetBool("chargingJump", false);
        }

        //when release space, jump
        if (Input.GetKeyUp("space"))
        {
            if (isGrounded())
            {
                rb.velocity = new Vector2(moveInput * walkSpeed, jumpValue);
                jumpValue = 0.0f;
                canDoubleJump = true;
                Invoke("ResetJump", 0.2f);
                anim.SetBool("chargingJump", false);
            }
            //canJump = true;
        }

        if (isWallSliding && Input.GetKeyDown("space"))
        {
            WallJump();
        }
        WallSlide();

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

        //if (!isWallJumping)
        //{
        //    Flip();
        //}
    }

    //private void FixedUpdate()
    //{
    //    if (!isWallJumping)
    //    {
    //        rb.velocity = new Vector2(moveInput * walkSpeed, rb.velocity.y);
    //    }
    //}

    private bool isGrounded()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y - 0.5f),
           new Vector2(0.9f, 0.4f), 0f, groundMask);
    }

    private bool isWalled()
    {
        return Physics2D.Raycast(transform.position, Vector2.right * Mathf.Sign(moveInput), 0.7f, wallMask);
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

    private void WallSlide()
    {
        if (isWalled() && !isGrounded() && moveInput != 0f)
        {
            Debug.Log("WallSliding");
            canDoubleJump = false;
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
            anim.SetBool("isSliding", true);
        }
        else
        {
            isWallSliding = false;
            anim.SetBool("isSliding", false);
            if(!isGrounded())
            {
                canDoubleJump = true;

            }
        }   
    }

    private void WallJump()
    {
        if (isFacingRight == true)
        {
            wallJumpingDirection = 1f;
        }
        else
        {
            wallJumpingDirection = -1f;
        }

        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;
            Debug.Log("WallJumpingCounter = " + wallJumpingCounter);

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if(Input.GetKeyDown("space") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;
            Debug.Log("WallJump");

            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }
    
    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void Flip()
    {
        if (isFacingRight && moveInput < 0f || !isFacingRight && moveInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector2 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    
}
