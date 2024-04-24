// Luna: add icons (moon level select), add sliders again (pitch/speed) + with UI numbers
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinConditions : MonoBehaviour
{

    // Set up reading inputs
    [SerializeField]
    private Slider slider1 = null;
    [SerializeField]
    private Slider slider2 = null;

    [SerializeField]
    private int win1 = 0;
    [SerializeField]
    private int win2 = 0;
    [SerializeField]
    private int radius1 = 0; // [Wthin radius amount is considered a win (ex. radius = 5 and win = 20, winning is 15-25)]
    [SerializeField]
    private int radius2 = 0;
    [SerializeField]
    private int levelToChangeTo = 0;

    // Start is called before the first frame update
    void Start()
    {
        slider1.onValueChanged.AddListener(delegate{winCheck();});
        slider2.onValueChanged.AddListener(delegate{winCheck();});
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void winCheck(){
        if(slider1.value > (win1 - radius1) && slider1.value < (win1 + radius1)){
            if(slider2.value > (win2 - radius2) && slider2.value < (win2 + radius2)){
                Debug.Log("Win Conditions Met!");
                UnityEngine.SceneManagement.SceneManager.LoadScene(levelToChangeTo);
            }
        }
    }
}
