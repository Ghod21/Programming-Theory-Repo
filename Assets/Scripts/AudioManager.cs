using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    // The script is for audio managing. Background music. Etc.

    public AudioSource audioSource;
    [SerializeField] UnityEngine.UI.Slider musicSlider;
    [SerializeField] UnityEngine.UI.Slider soundsSlider;
    public bool playMusic = true;
    [SerializeField] AudioClip[] audioClips;

    [SerializeField] private Light targetLight;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClips[0];
        audioSource.volume = DataPersistence.musicVolume * DataPersistence.soundAdjustment * 1.2f;
        if (musicSlider != null)
        {
            musicSlider.value = DataPersistence.musicVolume;
        }
        if (soundsSlider != null)
        {
            soundsSlider.value = DataPersistence.soundsVolume;
        }
        audioSource.Play();
    }
    public void BossMusicChangeStop()
    {
        //playMusic = false;
        //Debug.Log("Stopping audio source");
        //audioSource.Stop();
        //Debug.Log("Audio source stopped, changing clip");
        audioSource.clip = audioClips[1];
        audioSource.volume = DataPersistence.musicVolume * DataPersistence.soundAdjustment;
        Debug.Log("Clip changed");
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

        if (SceneManager.GetActiveScene().name == "Menu")
        {
            DataPersistence.musicVolume = musicSlider.value;
            DataPersistence.soundsVolume = soundsSlider.value;
            audioSource.volume = DataPersistence.musicVolume * DataPersistence.soundAdjustment;
        }
    }
    public void MusicSliderChange()
    {
        if (musicSlider != null)
        {
            musicSlider.interactable = true;
            DataPersistence.musicVolume = musicSlider.value;
        }
    }
    public void SoundsSliderChange()
    {
        if (soundsSlider != null)
        {
            soundsSlider.interactable = true;
            DataPersistence.soundsVolume = soundsSlider.value;
        }
    }

}
