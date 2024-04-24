using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SkipScene : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {
        // If x key is pressed, load next scene.
        if( Input.GetKeyDown(KeyCode.X) ){
            // Only specifying the scene name will load the scene in single mode.
            SceneManager.LoadScene("mainmenu", LoadSceneMode.Single);
        }

    }
}
