using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // The script is for audio managing. Background music. Etc.

    private AudioSource audioSource;
    public bool playMusic = true;
    private float volume = 0.5f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = volume;
        if (playMusic)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        if (playMusic && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (!playMusic && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
