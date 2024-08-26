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

    [Header("Ground Check Settings: ")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask wheresGround;

    [Header("Jump Settings: ")]
    [SerializeField]private float jumpForce = 45f;
    [SerializeField] private int jumpBufferFrames;
    private int jumpBufferCounter = 0;

    private Rigidbody2D rb;
    Animator anim;
    WizardStateList pState;

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
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariable();
        Flip();
        Move();
        Jump();
        
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
            if (jumpBufferCounter > 0 && Grounded())
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                pState.jumping = true;
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
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }

}
