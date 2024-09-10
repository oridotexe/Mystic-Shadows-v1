using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    [SerializeField] protected WizardController wizard;
    [SerializeField] protected float speed;

    [SerializeField] protected float damage;

    protected float recoilTimer;
    protected Rigidbody2D rb;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        wizard = WizardController.instance;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }
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
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        
        // gonna follow the hit direction 
        if(!isRecoiling )
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
            isRecoiling=true;   
        }
    }

    public void OnCollisionStay2D(Collision2D _other)
    {
        if (_other.gameObject.CompareTag("Wizard") && !WizardController.instance.pState.invincible)
        {
            Attack();
            WizardController.instance.HitStopTime(0, 5, 0.5f);
        }
    }

    public virtual void Attack()
    {
        WizardController.instance.TakeDamage(damage);
    }
}
