using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Control : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = .1f;

    // To be compatible with the previous code, I've created a new function that includes
    // the previous function while using a coroutine, so that the animation can play and
    // the scene changes with a delay
    public void NextScene(int id)
    {
        StartCoroutine(loadLevel(id));
    }

    IEnumerator loadLevel(int levelIndex){
        // Play animation
        transition.SetTrigger("start");

        // Wait
        yield return new WaitForSeconds(transitionTime);

        // Load scene
        SceneManager.LoadScene(levelIndex);
    }
}