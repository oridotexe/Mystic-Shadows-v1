using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    protected WizardController wizard;
    [SerializeField] protected float speed;

    [SerializeField] protected float damage;

    protected float recoilTimer;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator anim;

    protected enum EnemyStates
    {
        Mushroom_Idle,
        Mushroom_Flip,
        Fly_Idle,
        Fly_Chase,
        Fly_Stunned,
        Fly_Death
    }

    protected EnemyStates currentEnemyState;

    protected virtual EnemyStates GetCurrentEnemyState
    {
        get { return currentEnemyState; }
        set
        {
            if (currentEnemyState != value)
            {
                Debug.Log("nhjv"); 
                currentEnemyState = value;
                ChangeCurrentAnimation();
            }
        }
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        wizard = WizardController.instance;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
  
        if (isRecoiling)
        {
            if(recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
        else
        {
            UpdateEnemyStates();
        }
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        
        // gonna follow the hit direction 
        if(!isRecoiling )
        {
            rb.velocity = _hitForce * recoilFactor * _hitDirection;
            //rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
            //isRecoiling=true;   
        }
    }

    public void OnCollisionStay2D(Collision2D _other)
    {
        if (_other.gameObject.CompareTag("Wizard") && !WizardController.instance.pState.invincible && health>0)
        {
            Attack();
            WizardController.instance.HitStopTime(0, 5, 0.5f);
        }
    }

    public virtual void Attack()
    {
        WizardController.instance.TakeDamage(damage);
    }

    protected virtual void UpdateEnemyStates()
    {

    }

    protected virtual void ChangeCurrentAnimation()
    {

    }

    protected void ChangeState(EnemyStates _newState)
    {
        /*if (currentEnemyState != _newState)
        {
            Debug.Log($"Changing state from {currentEnemyState} to {_newState}");
            currentEnemyState = _newState;
        }*/
        GetCurrentEnemyState = _newState;  
    }
}
