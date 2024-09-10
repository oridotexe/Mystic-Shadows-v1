using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Unity.UI;
using Unity.UIElements;

public class WizardController : MonoBehaviour
{
    [Header("Horizontal Movement Settings: ")]
    [SerializeField] private float walkSpeed = 20f;
    [Space(5)]

    [Header("Vertical Movement Settings: ")]
    [SerializeField] private float jumpForce = 45f;
    [SerializeField] private int jumpBufferFrames;
    private float jumpBufferCounter = 0;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;
    private float gravity;
    [Space(5)]

    [Header("Ground Check Settings: ")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask wheresGround;
    [Space(5)]


    [Header("Dash Settings: ")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCoolDown;
    private bool ableDash = true, dashed;
    [Space(5)]

    [Header("Attack settings: ")]
    [SerializeField] Transform SideAttackTransform;
    [SerializeField] Transform UpAttackTransform;
    [SerializeField] Transform DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea;
    [SerializeField] Vector2 UpAttackArea;
    [SerializeField] Vector2 DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] private GameObject slashEffect;
    [SerializeField] float damage;
    [SerializeField] private bool attack = false;
    [SerializeField] private float timeBetweenAttack;
    private float timeSinceAttack;
    bool restoreTime;
    float restoreTimeSpeed;
    [Space(5)]

    [Header("Recoil settings: ")]
    [SerializeField] int recoilXSteps = 5;
    [SerializeField] int recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    private int stepsXRecoiled, stepsYRecoiled;

    [Header("Health settings: ")]
    public int health;
    public int maxHealth;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate OnHealthChangedCallback;
    [SerializeField] float timeToHeal;
    float healTimer;
    [Space(5)]

    [Header("Mana Settings")]
    [SerializeField] UnityEngine.UI.Image manaStorage;
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;
    [Space(5)]

    [SerializeField] GameObject bloodSpurt;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    [HideInInspector] public WizardStateList pState;

    // input variables
    private float xAxis, yAxis;


    public static WizardController instance;

    // Creates a singleton of the WizardController
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);  

    }

    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<WizardStateList>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing");
        }

        gravity = rb.gravityScale;
        Mana = mana;
        manaStorage.fillAmount = Mana;
        Health = maxHealth;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);  
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);  
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariable();

        if (pState.dashing) return;
        RestoreTimeScale();
        FlashWhileInvincible();
        Move();
        Heal();
        if (pState.healing) return;
        Flip();
        Jump();
        StartDash();
        Attack();
    }

    // recoils
    private void FixedUpdate()
    {
        if (pState.dashing || pState.healing) return;
        Recoil();
    }
    // Get the inputs of the game
    private void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
    }

    void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            pState.lookingRight = true;
        }

    }


    // Let the wizard's movement 
    private void Move()
    {
        if (pState.healing) rb.velocity = new Vector2(0, 0); // so it doesnt move
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        anim.SetBool("Walking", rb.velocity.x != 0 && Grounded());
    }

    void StartDash()
    {
        if (Input.GetButtonDown("Dash") && ableDash && !dashed)
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
        int _dir = pState.lookingRight ? 1 : -1;
        rb.velocity = new Vector2(_dir * dashSpeed, 0);
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
        Debug.Log("Is Grounded: " + isGrounded);
        return isGrounded;
    }


    // Wizard's jump movement 
    void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 3)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;

        }

        if (!pState.jumping && jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            pState.jumping = true;
            jumpBufferCounter = 0;
        }
        
        if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
        {
            pState.jumping = true;
            airJumpCounter++;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
          
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
            jumpBufferCounter = 0;
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

    // Attack 
    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (Input.GetButtonDown("Attack") && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");

            if (yAxis == 0 || (xAxis < 0 && Grounded()))
            {
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilYSpeed);
                Instantiate(slashEffect, SideAttackTransform.position, Quaternion.identity);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed);
                SlashEffectAngle(slashEffect, 80, UpAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed);
                SlashEffectAngle(slashEffect, -90, DownAttackTransform);
            }
        }
    }

    private void Hit(Transform _attackTranform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrenght)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTranform.position, _attackArea, 0, attackableLayer);
        List<Enemy> hitEnemies = new List<Enemy>();

        if(objectsToHit.Length > 0 )
        {
            _recoilDir = true;
        }

        for (int i = 0; i < objectsToHit.Length; i++)
        {   
           Enemy e = objectsToHit[i].GetComponent<Enemy>();
            if(e && !hitEnemies.Contains(e))
            {
                e.EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrenght);
                hitEnemies.Add(e);
                if (objectsToHit[i].CompareTag("Enemy")){
                    Mana += manaGain;
                }
            }
        }
    }

    // Stops the time once the player is hitten
    public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        if (_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        else
        {
            restoreTime = true;
        }

        Time.timeScale = _newTimeScale;
    }
    private void SlashEffectAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0,0, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);    
    }

    // recoil - retroceder cuando se enfrentan
    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2 (recoilXSpeed, 0);
            }
        }

        if (pState.recoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else
            {
                rb.velocity = new Vector2 (rb.velocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0; 
        }
        else
        {
            rb.gravityScale = gravity;
        }

        // Here stops the recoil
        if(pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;   
        }
        else
        {
            StopRecoilX();
        }

        if(pState.recoilingY && stepsXRecoiled < stepsYRecoiled)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if (Grounded())
        {
            StopRecoilY();
        }
    }

    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }

    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }

    // corroutine so it can stop taking damage
    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        anim.SetTrigger("TakeDamage");
        GameObject _bloodParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodParticles, 1.5f);
        yield return new WaitForSeconds(1f);
        pState.invincible = false;  
    }
    public void TakeDamage(float _damage)
    {
        Health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage()); 
    }

    public int Health
    {
        get { return health;}
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if(OnHealthChangedCallback != null)
                {
                    OnHealthChangedCallback.Invoke();
                }
            }
        }
    }

    // Coroutine that helps start the time again
    IEnumerator StartTimeAgain(float _delay)
    {
        yield return new WaitForSecondsRealtime(_delay);
        restoreTime = true;
    }

    // Starts the time again
    void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }

    void FlashWhileInvincible()
    {
        sr.material.color = pState.invincible ? Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f)) : Color.white;
    }

    // Heasls the wizard
    void Heal()
    {
        if (Input.GetButton("Healing") && Health < maxHealth && Mana > 0 && Grounded() && !pState.dashing && !pState.jumping)
        {
            pState.healing = true;
            anim.SetBool("Healing", true);
            //healing
            healTimer += Time.deltaTime;
            if (healTimer >= timeToHeal)
            {
                Health++;
                healTimer = 0;
            }
            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else
        {
            pState.healing = false;
            anim.SetBool("Healing", false);
            healTimer = 0;
        }
    }


    // it doesnt let us to heal always
    float Mana
    {
        get { return mana; }
        set
        {
            //if mana stats change
            if (mana != value)
            {
                mana = Mathf.Clamp(value, 0, 1);
                manaStorage.fillAmount = Mana;

            }
        }
    }

}
