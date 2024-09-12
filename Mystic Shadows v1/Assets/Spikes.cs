using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Wizard"))
        {
            StartCoroutine(RespawnPoint());
        }
    }

    IEnumerator RespawnPoint()
    {      
        WizardController.instance.pState.cutScene = true;
        WizardController.instance.pState.invincible = true;
        WizardController.instance.rb.velocity = Vector2.zero;
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(0.25f);  
        WizardController.instance.TakeDamage(1);
        yield return new WaitForSecondsRealtime(0.5f);  
        WizardController.instance.transform.position = GameManager.Instance.platformingRespawPoint;
        WizardController.instance.pState.cutScene = false;
        WizardController.instance.pState.invincible = false;
        Time.timeScale = 1;
    }

}
