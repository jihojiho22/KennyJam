using UnityEngine;

public class SimpleMusicPlayer : MonoBehaviour
{
    [Header("Music Settings")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float volume = 1.0f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loopMusic = true;
    
    private AudioSource audioSource;
    
    void Start()
    {
        // Get or create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Set up AudioSource
        audioSource.clip = backgroundMusic;
        audioSource.volume = volume;
        audioSource.loop = loopMusic;
        
        // Play music if enabled
        if (playOnStart && backgroundMusic != null)
        {
            audioSource.Play();
        }
    }
    
    // Simple public methods
    public void PlayMusic()
    {
        if (backgroundMusic != null)
            audioSource.Play();
    }
    
    public void StopMusic()
    {
        audioSource.Stop();
    }
    
    public void PauseMusic()
    {
        audioSource.Pause();
    }
    
    public void ResumeMusic()
    {
        audioSource.UnPause();
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }
} 