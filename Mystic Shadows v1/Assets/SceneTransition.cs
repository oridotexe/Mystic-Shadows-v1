using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{

    [SerializeField] private string transitionTo; 
    [SerializeField] private Transform startPoint; 
    [SerializeField] private Vector2 exitDirection; 
    [SerializeField] private float exitTime; 

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance.transitionedFromScene == transitionTo)
        {
            WizardController.instance.transform.position = startPoint.position;
            StartCoroutine(WizardController.instance.WalkIntoNewScene(exitDirection, exitTime));
        }
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Wizard"))
        {
            GameManager.Instance.transitionedFromScene = SceneManager.GetActiveScene().name;
            WizardController.instance.pState.cutScene = true;
            WizardController.instance.pState.invincible = true; 
            SceneManager.LoadScene(transitionTo);
        }
    }
}
