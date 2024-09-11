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
    private bool hasTransitioned = false;

    // Start is called before the first frame update
    void Start()
    {
        hasTransitioned = true;
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null.");
            return;
        }

        if (WizardController.instance == null)
        {
            Debug.LogError("WizardController.instance is null.");
            return;
        }

        if (startPoint == null)
        {
            Debug.LogError("startPoint is null.");
            return;
        }

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
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager.Instance is null.");
                return;
            }

            if (WizardController.instance == null)
            {
                Debug.LogError("WizardController.instance is null.");
                return;
            }

            GameManager.Instance.transitionedFromScene = SceneManager.GetActiveScene().name;
            WizardController.instance.pState.cutScene = true;
            WizardController.instance.pState.invincible = true;
            StartCoroutine(LoadSceneAfterDelay(transitionTo, 0.1f));
        }
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }


}
