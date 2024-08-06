using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // The script is for audio managing. Background music. Etc.

    public AudioSource audioSource;
    [SerializeField] UnityEngine.UI.Slider musicSlider;
    [SerializeField] UnityEngine.UI.Slider soundsSlider;
    public bool playMusic = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (musicSlider != null)
        {
            musicSlider.value = DataPersistence.musicVolume;
        }
        if (soundsSlider != null)
        {
            soundsSlider.value = DataPersistence.soundsVolume;
        }
        if (playMusic)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        audioSource.volume = DataPersistence.musicVolume;
        if (playMusic && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (!playMusic && audioSource.isPlaying)
        {
            audioSource.Stop();
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
