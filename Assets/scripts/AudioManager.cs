// AUDIO MANAGER COMPONENT
// Audio Manager class that controls the target song and the sample tracks for a level.

using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class AudioManager : MonoBehaviour
{
    public TargetSong targetFMOD;
    public ParentSample parentSampleEvent;

    // For work with volume Slider
    [SerializeField]
    private Slider targetSlider = null;
    [SerializeField]
    private Slider parentSampleSlider = null;
    [SerializeField]
    private Toggle targetToggle = null;
    [SerializeField]
    private Toggle parentSampleToggle = null;
    [SerializeField]
    private Button allPlayButton = null;
    [SerializeField]
    private Button samplePlayButton = null;
    [SerializeField]
    private Button targetPlayButton = null;

    

    // Width of each timeline or "track" for reference.
    public int timelineWidth = 1024;
    public int timelineHeight = 256;
    public int canvasHeight;

    private Boolean canvasInitialized = false;
    private int targetLength; // The length of the target event in milliseconds, does not change.

    public int speedDelta = 2; // between 0-100, middle is 75, no speed and pitch effect.
    public float pitchDelta = 0.005f;

   

    void Start()
    {
        targetLength = 0;
        canvasHeight = timelineHeight * 3; // For sake of reference, say the canvas is 3 times the track height.

        // Add listeners for each slider and toggle.
        targetToggle.onValueChanged.AddListener(delegate{ TargetSliderChange(); });
        parentSampleToggle.onValueChanged.AddListener(delegate{ ParentSliderChange(); });
        targetSlider.onValueChanged.AddListener(delegate { TargetSliderChange(); });
        parentSampleSlider.onValueChanged.AddListener(delegate { ParentSliderChange(); });
        allPlayButton.onClick.AddListener(delegate { playAllButton();});
        samplePlayButton.onClick.AddListener(delegate { playSampleButton();});
        targetPlayButton.onClick.AddListener(delegate { playTargetButton();});
    }

    void Update()
    {
        // If targetLength is 0, set when target event is ready.
        if (targetLength == 0 && targetFMOD.TargetEventReady())
        {
            SetSampleReferenceLengths();   
        }
        // Initialize the canvas and other timeline references of sample tracks if needed.
        if (!canvasInitialized)
        {
            InitalizeCanvas();   
        }

        // Check for input to play all songs
        CheckInputs();

        List<FMOD.Studio.EventInstance> _sampleEvents = parentSampleEvent.GetEventInstances();
 
    }

    // <summary>
    // Function that runs every time the target slider changes.
    // </summary>
    public void TargetSliderChange()
    {
        Debug.Log("In target slider func...\n");
        if(!targetToggle.isOn)
        {
            targetFMOD.GetTargetEvent().setParameterByNameWithLabel("Muted", "Not Muted");
            
            if(targetFMOD.GetTargetEvent().isValid())
            {
                targetFMOD.GetTargetEvent().setParameterByName("Volume", targetSlider.value);
                Debug.Log("Hello Luna here: " + targetFMOD.GetTargetEvent().setParameterByName("Volume", targetSlider.value));
            }
        }
        else
        {
            targetFMOD.GetTargetEvent().setParameterByName("Volume", 0);
            Debug.Log(targetSlider.value);
        } 
    }

    // <summary>
    // Function that runs every time the parent slider changes.
    // </summary>
    public void ParentSliderChange()
    {   
        List<FMOD.Studio.EventInstance> _sampleEvents = parentSampleEvent.GetEventInstances();
        if(!parentSampleToggle.isOn)
            foreach( FMOD.Studio.EventInstance sample in _sampleEvents ) 
                sample.setParameterByName("Volume", parentSampleSlider.value);
        else 
            _sampleEvents[0].setParameterByName("Volume", 0);
    }

    // <summary>
    // Set reference timeline length for the parent sample and it's events.
    // </summary>
    public void SetSampleReferenceLengths() 
    {
        targetLength = targetFMOD.GetTargetLength();

        // Set the reference length as 2x the targetLength, to allow room for stretch and shrink.
        parentSampleEvent.SetReferenceLength(targetLength * 2);

        Debug.Log("Setting Ref Length: " + (targetLength * 2));
    }

    // <summary>
    // Initialize the canvas and timeline width and heights.
    // </summary>
    public void InitalizeCanvas()
    {
        // Set timelines for parent event and each timeline of the parent event's sample tracks.
        parentSampleEvent.SetTimelines(timelineWidth, timelineHeight);

        // set canvas init to true.
        canvasInitialized = true;

        // Debug.Log("Setting timelines (w,h): (" + timelineWidth + ", " + timelineHeight + ")");
        // Debug.Log("Canvas Initilized (bool) : " + canvasInitialized);
    } 

    // <summary>
    // Checks all inputs to control events for sample and target playbacks.
    // || Space  || P      || O   ||
    // || Target || Sample || All ||
    // </summary>
    public void CheckInputs()
    {
        // Check for SPACE input for target playback.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            targetFMOD.TogglePlayback();
        }
        
        // Check for P input for sample playbacks.
        if (Input.GetKeyDown(KeyCode.P))
        {
            parentSampleEvent.TogglePlayback();
        }

        // Check for A (o like octopus) input to play all.
        if (Input.GetKeyDown(KeyCode.A))
        {
            targetFMOD.TogglePlayback();
            parentSampleEvent.TogglePlayback();
        }
        // Check for Return key input to stop all amd reset timelines.
        if (Input.GetKeyDown(KeyCode.Return))
        {
            targetFMOD.StopPlayback();
            parentSampleEvent.StopPlayback();
        }

       
        // Check for Right key for speed up
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //parentSampleEvent.AdjustPitch(pitchDelta);
            parentSampleEvent.AdjustSpeed(speedDelta);
        }
        // Check for Left key for speed down.
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            parentSampleEvent.AdjustSpeed(-speedDelta);
        }

        // Check for Up key for pitch up
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            parentSampleEvent.AdjustPitch(pitchDelta);
        }
        // Check for down key for pitch down.
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            parentSampleEvent.AdjustPitch(-pitchDelta);
        }

    
    }

    // <summary>
    // Attached to the Unity button to play all events if none are playing- otherwise stop playback.
    // </summary>
    public void playAllButton(){
        if(!parentSampleEvent.EventsPlaying() && !targetFMOD.IsPlaying()){
            targetFMOD.TogglePlayback();
            parentSampleEvent.TogglePlayback();
        }
        else{
            targetFMOD.StopPlayback();
            parentSampleEvent.StopPlayback();
        }
    }
        public void playSampleButton(){
        if(!parentSampleEvent.EventsPlaying()){
            parentSampleEvent.TogglePlayback();
        }
        else{
            parentSampleEvent.StopPlayback();
        }
    }
        public void playTargetButton(){
        if(!targetFMOD.IsPlaying()){
            targetFMOD.TogglePlayback();
        }
        else{
            targetFMOD.StopPlayback();
        }
    }
}
