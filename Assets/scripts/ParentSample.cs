// PARENT SAMPLE  COMPONENT
// The Parent Sample for all the fmod sample events for a given level. Controlled by AudioManager.

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using TMPro;

public class ParentSample : MonoBehaviour
{

    public GameObject arrow = null;
    public Camera cam = null;

    // List of SampleTrack objects for each sample, should be properly set up in inspector.
    public List<SampleTrack> _sampleTracks;

    // The sliders to control/display pitch and speed
    [SerializeField]
    private Slider pitchSlider = null;
    [SerializeField]
    private Slider speedSlider = null;
    [SerializeField]
    private TMP_Text pitchText;
    [SerializeField]
    private TMP_Text speedText;
    

    ///////////// FMOD DSP stuff
    public List<FMODUnity.EventReference> _sampleEventPaths; // List of FMOD event references
    public int _windowSize = 512;
    public FMOD.DSP_FFT_WINDOW _windowShape = FMOD.DSP_FFT_WINDOW.RECT;

    private List<FMOD.Studio.EventInstance> _sampleEvents; // List of FMOD event instances

    private FMOD.Channel _channel;
    private FMOD.ChannelGroup _channelGroup;
    private FMOD.DSP _dsp;
    private FMOD.DSP_PARAMETER_FFT _fftparam;
    private FMOD.DSP _fft;

    // Length of the Target song for reference (in milliseconds).
    private int referenceLength;

    [SerializeField]
    private int timelineWidth; // Represents a width that is equal to the reference length (in milliseconds).
    [SerializeField]
    private int timelineHeight;

    private float arrowoffsetx;
    private float originOffset;
    private float arrowOriginOffset;

    private Boolean tracksCreated;
    private Boolean timelineSet;

    // Assuming these are your pitch bounds, adjust as needed.
    private const float minPitch = 0.5f;
    private const float maxPitch = 1.5f;
    // Our bounds for the speed parameter.
    private const int minSpeed = 0;
    private const int maxSpeed = 150;

    private float originalPitch;


    private void Start()
    {   
        
        // Reference length starts at 0 until set by audio manager.
        referenceLength = 0;
        tracksCreated = false;
        timelineSet = false;

        // Initialize sample events.
        _sampleEvents = new List<FMOD.Studio.EventInstance>();
        

        // Prepare FMOD event, sets _parentEvent.
        PrepareSampleEvents();

        // Set cursor to origin.
        CursorToOrigin();
        originOffset = Math.Abs(arrow.transform.position.x);
        arrowOriginOffset = originOffset + arrowoffsetx;

        originalPitch = GetPitch();

        //Check Slider Values
        pitchSlider.onValueChanged.AddListener(delegate{AdjustPitchSlider();});
        speedSlider.onValueChanged.AddListener(delegate{AdjustSpeedSlider();});
    }

    // On destroy, release the events.
    void OnDestroy()
    {
        Debug.Log("--- Destroying sample events.\n");

        foreach (FMOD.Studio.EventInstance sampleEvent in _sampleEvents)
        {
            
            sampleEvent.setUserData(IntPtr.Zero);
            sampleEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            sampleEvent.release();
        }
    }

    private void Update()
    {
        // If referenceLength is not 0, prepare the Sample Track "waveforms".
        if (timelineSet && !tracksCreated && referenceLength > 0)
        {
            // Prepare Sample Tracks
            PrepareSampleTracks();
        }


        // Update the cursor position for playback.
        UpdateCursorPosition();
    }

    // Release all the valid sample events.
    void ReleaseEvents() {
        Debug.Log("--- Releasing sample events.\n");

        foreach(FMOD.Studio.EventInstance sampleEvent in _sampleEvents) {
            if(sampleEvent.isValid()) {
                //sampleEvent.setUserData(IntPtr.Zero);
                sampleEvent.release();
                // Always stop after manual release.
                //sampleEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    // Sets the timeline width and height for each sample track belonging to a sample event.
    public void SetTimelines(int width, int height)
    {
        // Set the timeline width and height for this parent event.
        this.SetTimelineWidth(width);
        this.SetTimelineHeight(height);

        foreach (SampleTrack sampleTrack in _sampleTracks)
        {
            sampleTrack.SetTimelineWidth(width);
            sampleTrack.SetTimelineHeight(height);
        }

        // Change timelineSet to true.
        timelineSet = true;
    }

    //////////////////////////////////



    // Sets the cursor to its origin position.
    public void CursorToOrigin()
    {
        // Set all event timeline positions to 0.
        ResetEvents();

        // Adjust arrow and waveform sprite.
        // arrow.transform.position = new Vector3(, 0f, 0f);

        arrowoffsetx = -(arrow.GetComponent<SpriteRenderer>().size.x / 2f);
    }

    // Updates the cursor while the event is playing.
    public void UpdateCursorPosition()
    {

        // Get any sample event and track for reference, this case first one.
        FMOD.Studio.EventInstance sampleEvent = _sampleEvents[0];
        SampleTrack sampleTrack = _sampleTracks[0];

        // Get the adjustedWidth from the longest event length * the width per ms.
        float adjustedWidth = GetLongestEventLength() * sampleTrack.GetWidthPerMillisecond();

        // Get the position and length of the event.
        sampleEvent.getTimelinePosition(out int currentPosition); // both in milliseconds
        int sampleLength = GetSampleLength(sampleEvent); // in milliseconds

        // Convert milliseconds to seconds for more accurate calculation
        float currPossSeconds = (float)currentPosition / 1000;
        float eventLengthSeconds = (float)sampleLength / 1000;

        // Calculate the offset based on the current position and event length
        float xoffset = ((currPossSeconds / eventLengthSeconds) * (adjustedWidth / 100)); // For some reason going too fast.
        xoffset -= originOffset; // originOffset is positive.
        //Debug.Log("xoffset: " + xoffset);

        // Update arrow position
        arrow.transform.position = new Vector3(xoffset, arrow.transform.position.y, arrow.transform.position.z);
    }

    // Helper function that sets the timelineHeight from parent component.
    public void SetTimelineWidth(int width)
    {
        timelineWidth = width;
    }

    // Helper function that sets the timelineHeight from parent component.
    public void SetTimelineHeight(int height)
    {
        timelineHeight = height;
    }

    ////////////////////// Playback Functions //////////////////////


    // Helper function to toggle the play/pause state of all sample events.
    public void TogglePlayback()
    {
        // Check the sample events are valid.
        if (SampleEventsValid())
        {
            // If events haven't started.
            if (EventsStopped())
            {
                Debug.Log("Starting events... \n");

                StartEvents();
            }
            // If event is playing, pause the event.
            else if (EventsPlaying())
            {
                Debug.Log("Toggling the pause state for all events... \n");

                PauseEvents();
            }
        }
    }

    // Helper function to stop event playback altogether after S/s is pressed.
    public void StopPlayback()
    {
        // Check the sample events are valid.
        if (SampleEventsValid())
        {
            // If the events are playing, stop all events.
            if (EventsPlaying())
            {
                Debug.Log("Stopping all the events... \n");

                StopEvents();
                // Put the cursor to the origin.
                CursorToOrigin();
            }

        }
    }

    // Helper function to play all events in the _sampleEvents list.
    public void StartEvents()
    {
        foreach (FMOD.Studio.EventInstance sampleEvent in _sampleEvents)
        {
            PlayEvent(sampleEvent);
        }
    }

    // Helper function to play a given event.
    void PlayEvent(FMOD.Studio.EventInstance sampleEvent)
    {
        sampleEvent.setPaused(false);
        sampleEvent.start();

        // Set the channel group to the event instance group instead of master.
        sampleEvent.getChannelGroup(out _channelGroup);
        _channelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, _fft);
    }

    // Helper function to stop all events in the _sampleEvents list.
    public void StopEvents()
    {
        foreach (FMOD.Studio.EventInstance sampleEvent in _sampleEvents)
        {
            StopEvent(sampleEvent);
        }
    }

    // Helper function to stop a given event.
    void StopEvent(FMOD.Studio.EventInstance sampleEvent)
    {
        sampleEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        //sampleEvent.release(); // Don't do this here. Won't allow to play again.  
    }

    // Helper function to pausey all events in the _sampleEvents list.
    public void PauseEvents()
    {
        foreach (FMOD.Studio.EventInstance sampleEvent in _sampleEvents)
        {
            PauseEvent(sampleEvent);
        }
    }

    // Helper function to pause a given event.
    void PauseEvent(FMOD.Studio.EventInstance sampleEvent)
    {
        sampleEvent.getPaused(out Boolean paused);
        sampleEvent.setPaused(!paused);
    }

    // Checks to see if the list of sample events is valid.
    public Boolean SampleEventsValid()
    {
        foreach (FMOD.Studio.EventInstance sampleEvent in _sampleEvents)
        {
            if (!EventIsValid(sampleEvent))
            {
                return false;
            }
        }

        // Return true if none of them are false.
        return true;
    }

    // Checks to see if a given event is valid.
    Boolean EventIsValid(FMOD.Studio.EventInstance sampleEvent)
    {
        return sampleEvent.isValid();
    }

    // Returns true if any of the sample events are playing.
    public Boolean EventsPlaying()
    {
        foreach (FMOD.Studio.EventInstance sampleEvent in _sampleEvents)
        {
            if (IsPlaying(sampleEvent))
            {
                return true;
            }
        }

        // Return false if none are playing.
        return false;
    }

    // Returns true if any of the sample events are stopped.
    Boolean EventsStopped()
    {
        foreach (FMOD.Studio.EventInstance sampleEvent in _sampleEvents)
        {
            if (IsStopped(sampleEvent))
            {
                return true;
            }
        }

        // Return false if none are stopped.
        return false;
    }

    // Returns true if any of the sample events are paused.
    Boolean EventsPaused()
    {
        foreach (FMOD.Studio.EventInstance sampleEvent in _sampleEvents)
        {
            if (IsPaused(sampleEvent))
            {
                return true;
            }
        }

        // Return false if none are paused.
        return false;
    }


    // Helper function to tell whether the event playback is playing.
    Boolean IsPlaying(FMOD.Studio.EventInstance sampleEvent)
    {
        return PlaybackState(sampleEvent) == FMOD.Studio.PLAYBACK_STATE.PLAYING;
    }

    // Helper function to tell whether the event playback is stopped.
    Boolean IsStopped(FMOD.Studio.EventInstance sampleEvent)
    {
        FMOD.Studio.PLAYBACK_STATE ps = PlaybackState(sampleEvent);
        return ps == FMOD.Studio.PLAYBACK_STATE.STOPPED || ps == FMOD.Studio.PLAYBACK_STATE.STOPPING;
    }

    Boolean IsPaused(FMOD.Studio.EventInstance sampleEvent)
    {
        sampleEvent.getPaused(out Boolean paused);

        return paused == true;
    }

    // Helper function to get the playback state of the event.
    FMOD.Studio.PLAYBACK_STATE PlaybackState(FMOD.Studio.EventInstance sampleEvent)
    {
        FMOD.Studio.PLAYBACK_STATE pS;
        sampleEvent.getPlaybackState(out pS);
        return pS;
    }

    // Sets all time line positions for each event to a given position (in milliseconds).
    public void SetTimelinePositions(int position = 0)
    {
        // By default sets timeline position to 0.
        foreach (FMOD.Studio.EventInstance sampleEvent in _sampleEvents)
        {
            sampleEvent.setTimelinePosition(position);
        }

        Debug.Log("Timeline Position set to: (" + position + ") for all events...\n");
    }

    // Resets tracks to timeline position of 0.
    public void ResetEvents()
    {
        SetTimelinePositions(0);
    }


    // Helper function to speed event up by delta f.
    public void AdjustSpeed(int delta)
    {
        FMOD.Studio.EventInstance sampleEvent = _sampleEvents[0];
        String speedParameter = "Speed";
        
        int currSpeed = GetSpeed(); 

        // If curr speed is valid, adjust it by delta value.
        if(currSpeed + delta < 150 && currSpeed + delta > 0)
            if(currSpeed != -1){
                Debug.Log("Delta: " + delta);
                Debug.Log("currSpeed: " + currSpeed);   

                int speedValue = currSpeed + delta;
                sampleEvent.setParameterByName(speedParameter, (float)speedValue);

                // Change the saturation of the texture.
                _sampleTracks[0].ChangeTextureSaturation((float)(speedValue));
                speedSlider.value = speedValue;

                speedText.text = (speedValue - 75) + "x";
            } 

    }
    // If speed is adjusted via slider
        public void AdjustSpeedSlider()
    {
        int currSpeed = GetSpeed();    
        Debug.Log("speed changed");
        int sliderValue = (int) speedSlider.value;
        FMOD.Studio.EventInstance sampleEvent = _sampleEvents[0];
        String speedParameter = "Speed";

        // If curr speed is valid, adjust it by delta value.
        if(sliderValue != -1){
            sampleEvent.setParameterByName(speedParameter, (float)sliderValue);
            // Change the saturation of the texture.
            _sampleTracks[0].ChangeTextureSaturation(sliderValue);
            speedText.text = "" + (sliderValue-75) + "x";
        } 

    }

    int GetSpeed()
    {
        FMOD.Studio.EventInstance sampleEvent = _sampleEvents[0];
        String speedParameter = "Speed";

        // If event is valid, get the events "Speed" parameter value.
        if(EventIsValid(sampleEvent)) {
            sampleEvent.getParameterByName(speedParameter, out float currSpeed);

            Debug.Log("Current Speed: " + currSpeed);

            return (int)currSpeed;
        }

        return -1;

    }

    // Helper function to pitch event up by delta f.
    public void AdjustPitch(float delta)
    {
        FMOD.Studio.EventInstance sampleEvent = _sampleEvents[0];
        
        float currPitch = GetPitch();       

        // If curr speed is valid, adjust it by delta value.
        if(EventIsValid(sampleEvent)){
            float pitchValue = currPitch + delta;
            
            sampleEvent.setPitch(pitchValue);

            // New pitch value for color correction.
            float pitchChange = (pitchValue - originalPitch) * 0.1f; // Example scale factor, adjust as n
            

            // Change texture color based on pitch.
            _sampleTracks[0].ChangeTextureColor(pitchChange);

            // Update the Slider
            pitchSlider.value = pitchValue;
            float pitchValueConverted = pitchValue * 100;
            pitchText.text = "" + (int) pitchValueConverted + "%";
        } 

    }
    // Adjusting Pitch if it was done by slider
    public void AdjustPitchSlider()
    {
        float sliderValue = pitchSlider.value;
        FMOD.Studio.EventInstance sampleEvent = _sampleEvents[0];
        Debug.Log("Inside AdjustPitchSlider");
        float currPitch = GetPitch();       

        // If curr speed is valid, adjust it by delta value.
        if(EventIsValid(sampleEvent)){            
            sampleEvent.setPitch(sliderValue);

            // New pitch value for color correction.
            float pitchChange = (sliderValue - originalPitch) * 0.1f; // Example scale factor, adjust as n
            

            // Change texture color based on pitch.
            _sampleTracks[0].ChangeTextureColor(sliderValue);
            float pitchValueConverted = sliderValue * 100;
            pitchText.text = "" + (int) pitchValueConverted + "%";
        } 

    }
    float GetPitch()
    {
        FMOD.Studio.EventInstance sampleEvent = _sampleEvents[0];
        sampleEvent.getPitch(out float currPitch);

        Debug.Log("Current Pitch: " +currPitch);

        return currPitch;

    }



    // Gets the event description for the given sample event.
    public FMOD.Studio.EventDescription GetEventDescription(FMOD.Studio.EventInstance sampleEvent)
    {
        sampleEvent.getDescription(out FMOD.Studio.EventDescription sampleDesc);

        return sampleDesc;
    }

    // Gets the event description for the given sample event.
    public int GetSampleLength(FMOD.Studio.EventInstance sampleEvent)
    {
        FMOD.Studio.EventDescription sampleDesc = GetEventDescription(sampleEvent);
        sampleDesc.getLength(out int sampleLength);

        return sampleLength;
    }

    // Gets the length of the longest event (in milliseconds).
    int GetLongestEventLength()
    {
        int length = -1;
        foreach (FMOD.Studio.EventInstance sampleEvent in _sampleEvents)
        {
            int currentLength = GetSampleLength(sampleEvent);

            length = (length < 0 || currentLength > length) ? currentLength : length;
        }

        return length;
    }

    // Getter function for all the sampleEvents
    public List<FMOD.Studio.EventInstance> GetEventInstances(){
        return _sampleEvents;
    }

    ////////////////////// Playback Functions //////////////////////


    ////////////////////// Event Prepare and Waveform //////////////////////

    // Helper function that sets the reference length from the Audio Manager's target node.
    public void SetReferenceLength(int length)
    {
        referenceLength = length;
    }

    // Prepare FMOD Event Instances from the list of sample events.
    private void PrepareSampleEvents()
    {
        // Prepare FMOD event instances.
        for (int i = 0; i < _sampleEventPaths.Count; i++)
        {
            // Create the event instance.
            FMOD.Studio.EventInstance eventInstance = FMODUnity.RuntimeManager.CreateInstance(_sampleEventPaths[i]);
            eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject.transform));
            _sampleEvents.Add(eventInstance);

            // Set the sample tracks event instance as well.
            _sampleTracks[i].SetSampleEvent(eventInstance);

        }

        // Create the FFT dsp, set window type, and window size.
        FMODUnity.RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out _dsp);
        _dsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)_windowShape);
        _dsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, _windowSize * 2);
    }


    // Prepares the Sample tracks in order to create each of their waveforms.
    private void PrepareSampleTracks()
    {
        // For each sample track in the list, sets width per millisecond and creates the waveform texture.
        foreach (SampleTrack sampleTrack in _sampleTracks)
        {
            // Set it's origin based on the y position of the sample track, everything else is this transfom.
            sampleTrack.SetOriginVector(transform.position.x, sampleTrack.transform.position.y, transform.position.z);
            // Set the width per millisecond.
            sampleTrack.SetWidthPerMillisecond(referenceLength);

            sampleTrack.PrepareSampleTrack(referenceLength);
        }

        tracksCreated = true;
    }



}