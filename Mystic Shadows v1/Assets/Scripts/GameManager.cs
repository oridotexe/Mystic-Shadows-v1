using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditorInternal;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string transitionedFromScene;
    public Vector2 platformingRespawPoint;
    public Vector2 respawnPoint;
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void RespawnWizard()
    {
        Debug.Log("RespawnWizard called");

        if (WizardController.instance == null)
        {
            Debug.LogError("WizardController.instance is null.");
            return;
        }

        respawnPoint = platformingRespawPoint;
        WizardController.instance.transform.position = respawnPoint;
        StartCoroutine(UIManager.Instance.DesactiveDeathScreen());
        WizardController.instance.Respawned();
    }


}
