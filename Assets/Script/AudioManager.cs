using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton pattern
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    
    [Header("Sound Effects")]
    public AudioSource sfxSource;
    public AudioClip buttonSound;
    
    private bool soundOn = true;
    
    private void Awake()
    {
        // Keep this object alive across scene loads
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create audio sources if needed
            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();
                
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();
                
            // Configure sources
            musicSource.loop = true;
            
            // Load sound settings
            soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        }
        else
        {
            // Another AudioManager already exists, destroy this one
            Destroy(gameObject);
        }
    }
    
    public void PlayMenuMusic()
    {
        if (menuMusic != null && (musicSource.clip != menuMusic || !musicSource.isPlaying))
        {
            musicSource.clip = menuMusic;
            musicSource.Play();
        }
    }
    
    public void PlayGameMusic()
    {
        if (gameplayMusic != null && musicSource.clip != gameplayMusic)
        {
            musicSource.clip = gameplayMusic;
            musicSource.Play();
        }
    }
    
    public void PlayButtonSound()
    {
        if (soundOn && buttonSound != null)
        {
            sfxSource.PlayOneShot(buttonSound);
        }
    }
    
    public void SetSound(bool isOn)
    {
        soundOn = isOn;
        
        // Debug to verify this gets called
        Debug.Log("AudioManager.SetSound: " + isOn);
        
        // Apply to all sources
        if (musicSource != null)
        {
            musicSource.mute = !soundOn;
        }
        
        if (sfxSource != null)
        {
            sfxSource.mute = !soundOn;
        }
    }
}