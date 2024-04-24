using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainStory : MonoBehaviour
{
    
    void OnEnable() 
    {
        // Only specifying the scene name will load the scene in single mode.
        SceneManager.LoadScene("mainmenu", LoadSceneMode.Single);
    }

}
