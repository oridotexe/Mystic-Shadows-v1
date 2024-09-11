using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : Enemy
{

    [SerializeField] private float chaseDistance;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Fly_Idle);
    }

    protected override void UpdateEnemyStates()
    {
        float _dist = Vector2.Distance(transform.position, WizardController.instance.transform.position);
        switch (currentEnemyState)
        {
            case EnemyStates.Fly_Idle:
                if(_dist < chaseDistance)
                {
                    ChangeState(EnemyStates.Fly_Chase); 
                }
                break;

            case EnemyStates.Fly_Chase:
                rb.MovePosition(Vector2.MoveTowards(transform.position, WizardController.instance.transform.position, Time.deltaTime * speed));
                break;

            case EnemyStates.Fly_Stunned:
                break;
            case EnemyStates.Fly_Death:
                break;
        }
    }

    void FlipFly()
    {
       
    }
}
