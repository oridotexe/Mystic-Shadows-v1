using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WizardController : MonoBehaviour
{
    [Header("Horizontal Movement Settings: ")]
    [SerializeField] private float walkSpeed = 20f;
    private float xAxis;
    [Space(5)]

    [Header("Ground Check Settings: ")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask wheresGround;
    [Space(5)]

    [Header("Jump Settings: ")]
    [SerializeField]private float jumpForce = 45f;
    [SerializeField] private int jumpBufferFrames;
    private float jumpBufferCounter = 0;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;
    [Space(5)]

    [Header("Dash Settings: ")]
    private bool ableDash = true;
    private bool dashed;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCoolDown;
    [Space(5)]

    private Rigidbody2D rb;
    Animator anim;
    WizardStateList pState;
    private float gravity;

    public static WizardController instance;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<WizardStateList>();   

        anim = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing");
        }

        gravity = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariable();
        if(pState.dashing) return;
        Flip();
        Move();
        Jump();
        StartDash();
        
    }

    // Get the inputs of the game
    private void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        // Debug.Log("xAxis: " + xAxis);
    }

    void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
        }
        else if ( xAxis > 0) 
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
  
    }


    // Let the wizard's movement 
    private void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        anim.SetBool("Walking", rb.velocity.x != 0 && Grounded()); 
    }

    void StartDash()
    {
        if(Input.GetButtonDown("Dash") && ableDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }

        if (Grounded())
        {
            dashed = false;
        }

    }


    // Coretune that pauses the execution of the code at certain points to help the dash animation
    IEnumerator Dash()
    {
        ableDash = false;
        pState.dashing = true;
        anim.SetTrigger("Dashing");
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCoolDown);
        ableDash = true;
    }


    // Detects if the wizard is on the ground
    public bool Grounded()
    {
        bool isGrounded = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, wheresGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, wheresGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, wheresGround);

        //Debug.Log("Is Grounded: " + isGrounded);
        return isGrounded;
    }


    // Wizard's jump movement 
    void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;
     
        }

        if (!pState.jumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                pState.jumping = true;
            }
            else if(!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                pState.jumping = true;
                airJumpCounter++;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        anim.SetBool("Jumping", !Grounded());
    }

    // Updates the position of Wizard's jump movement 
    void UpdateJumpVariable()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }


        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter = jumpBufferCounter - Time.deltaTime * 10;
        }
    }

}
