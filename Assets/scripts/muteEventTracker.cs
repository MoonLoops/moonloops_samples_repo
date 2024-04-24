using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;

public class ToggleSound : MonoBehaviour
{

    public FMODUnity.EventReference _eventPath;
    private FMOD.Studio.EventInstance _event;
    Toggle trackToggle;
    public

    // Use this for initialization
    void Start()
    {
        trackToggle = GetComponent<Toggle>();
        //Add listener for when the state of the Toggle changes, and output the state
        trackToggle.onValueChanged.AddListener(delegate {
            ButtonClick(trackToggle);
        });

    }

    public void ButtonClick(Toggle _toggle)
    {
        Debug.Log("Toggled: " + _toggle.isOn);
    }
}