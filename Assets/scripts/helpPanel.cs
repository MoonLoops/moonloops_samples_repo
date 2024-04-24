 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class helpPanel : MonoBehaviour
{
    public GameObject panel;
    public GameObject arrow;
    public GameObject parentsample;

    // If panel is on, turn off. If it's off, turn on.
    public void togglePanel(){
        if(panel != null)
            if(panel.activeSelf)
                panel.SetActive(false);
            else
                panel.SetActive(true);
        if(arrow != null)
            if(arrow.activeSelf)
                arrow.SetActive(false);
            else
                arrow.SetActive(true);
        if(parentsample != null)
            if(parentsample.activeSelf)
                parentsample.SetActive(false);
            else
                parentsample.SetActive(true);
        
    }
}
