using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance { get; private set; }
    [SerializeField] GameObject deathScreen;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    public IEnumerator ActivateDeathScreen()
    {
        yield return new WaitForSeconds(0.3f);
        deathScreen.SetActive(true);
    }

    public IEnumerator DesactiveDeathScreen()
    {
        yield return new WaitForSeconds(0.3f);
        deathScreen.SetActive(false);       
    }
}


