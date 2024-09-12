using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Clown : Enemy
{
    float timer;
    [SerializeField] private float flipWaitTime;
    [SerializeField] private float checkX;
    [SerializeField] private float checkY;
    [SerializeField] private LayerMask whatIsGround;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 5f;
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
    }

    protected override void UpdateEnemyStates()
    {
        if (health <= 0)
        {
            Destroy(gameObject, 0.05f);
        }

        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Clown_Idle:

                Vector3 _checkStart = transform.localScale.x > 0 ? new Vector3(checkX, 0) : new Vector3(-checkX, 0);
                Vector2 _wallCheckDir = transform.localScale.x > 0 ? transform.right : -transform.right;

                if (!Physics2D.Raycast(transform.position + _checkStart, Vector2.down, checkY, whatIsGround)
                    || Physics2D.Raycast(transform.position, _wallCheckDir, checkX, whatIsGround))
                {
                    ChangeState(EnemyStates.Clown_Flip);
                }

                if (transform.localScale.x > 0)
                {
                    rb.velocity = new Vector2(speed, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(-speed, rb.velocity.y);
                }

                break;

            case EnemyStates.Clown_Flip:
                timer += Time.deltaTime;
                if (timer > flipWaitTime)
                {
                    timer = 0;
                    transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
                    ChangeState(EnemyStates.Clown_Idle);
                }
                break;

        }
    }
}

