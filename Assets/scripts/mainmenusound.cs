using UnityEngine;

public class SoundEffectPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        audioSource.Play();
    }
}