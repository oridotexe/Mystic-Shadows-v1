using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class Fly : Enemy
{

    [SerializeField] private float chaseDistance;
    [SerializeField] private float stunDuration;

    float timer;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Fly_Idle);
    }

    protected override void UpdateEnemyStates()
    {
        float _dist = Vector2.Distance(transform.position, WizardController.instance.transform.position);
       // Debug.Log("estado " + GetCurrentEnemyState);
        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Fly_Idle:
                rb.velocity = new Vector2(0, 0);
                if(_dist < chaseDistance)
                {              
                    ChangeState(EnemyStates.Fly_Chase); 
                }
                break;

            case EnemyStates.Fly_Chase:
                rb.MovePosition(Vector2.MoveTowards(transform.position, WizardController.instance.transform.position, Time.deltaTime * speed));
                FlipFly();
                if(_dist > chaseDistance)
                {
                    ChangeState(EnemyStates.Fly_Idle);
                }

                break;

            case EnemyStates.Fly_Stunned:
                timer += Time.deltaTime;
                Debug.Log("enttro stunned");
                if (timer > stunDuration)
                {
                    ChangeState(EnemyStates.Fly_Idle);
                    timer = 0;
                }
                break;
            case EnemyStates.Fly_Death:
                //rb.gravityScale = 12f;
                Destroy(gameObject, Random.Range(5,10));
                break;
        }
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);   
        if(health > 0)
        {
            ChangeState(EnemyStates.Fly_Stunned);
        }
        else
        {
            ChangeState(EnemyStates.Fly_Death);
        }
    }
    void FlipFly()
    {
       /*if(WizardController.instance.transform.position.x < transform.position.x)
        {
            sr.flipX = true;
        }
        else
        {
            sr.flipX=false; 
        }*/

       sr.flipX = WizardController.instance.transform.position.x < transform.position.x;
    }

    protected override void ChangeCurrentAnimation()
    {
        anim.SetBool("Idle", GetCurrentEnemyState == EnemyStates.Fly_Idle);
        anim.SetBool("Chase", GetCurrentEnemyState == EnemyStates.Fly_Chase);
        anim.SetBool("Chase", GetCurrentEnemyState == EnemyStates.Fly_Chase);
        anim.SetBool("Stunned", GetCurrentEnemyState == EnemyStates.Fly_Stunned);

        if(GetCurrentEnemyState == EnemyStates.Fly_Death)
        {
            anim.SetTrigger("Death");
        }
    }
}
