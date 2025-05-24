using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("Audio Sources")]
    private AudioSource musicSource;

    // Keep track of state
    private bool isMusicEnabled = true;
    private string currentScene;

    private void Awake()
    {
        // Singleton pattern to ensure only one MusicManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create audio source
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.5f;
            
            // Load saved settings
            isMusicEnabled = PlayerPrefs.GetInt("SoundOn", 1) == 1;
            musicSource.mute = !isMusicEnabled;
            
            // Listen for scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // Destroy duplicate
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Change music based on scene
        if (scene.name == "MainMenu")
        {
            PlayMenuMusic();
        }
        else
        {
            PlayGameMusic();
        }
    }

    public void PlayMenuMusic()
    {
        if (menuMusic != null && (musicSource.clip != menuMusic || !musicSource.isPlaying))
        {
            musicSource.clip = menuMusic;
            musicSource.Play();
            Debug.Log("Playing menu music");
        }
    }

    public void PlayGameMusic()
    {
        if (gameMusic != null && (musicSource.clip != gameMusic || !musicSource.isPlaying))
        {
            musicSource.clip = gameMusic;
            musicSource.Play();
            Debug.Log("Playing game music");
        }
    }

    public void SetMusicEnabled(bool enabled)
    {
        Debug.Log("MusicManager.SetMusicEnabled: " + enabled);
        
        isMusicEnabled = enabled;
        
        // Ensure we have an audio source
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
        }
        
        // Set mute state
        musicSource.mute = !enabled;
        
        // Force play if unmuting and not playing
        if (enabled && !musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.Play();
        }
    }

    public bool IsMusicEnabled()
    {
        return isMusicEnabled;
    }

    public void TestSound()
    {
        // Force play a sound to test
        if (musicSource != null)
        {
            // Save current state
            bool wasMuted = musicSource.mute;
            AudioClip prevClip = musicSource.clip;
            bool wasPlaying = musicSource.isPlaying;
            
            // Unmute temporarily and play
            musicSource.mute = false;
            
            // Play current clip or menu music
            if (!musicSource.isPlaying)
            {
                if (menuMusic != null) 
                {
                    musicSource.clip = menuMusic;
                    musicSource.Play();
                    Debug.Log("TEST: Playing menu music");
                }
            }
            
            // Log the state
            Debug.Log("TEST: Music source is playing: " + musicSource.isPlaying);
            Debug.Log("TEST: Music source volume: " + musicSource.volume);
            
            // Restore mute state after 1 second if it was muted
            if (wasMuted)
            {
                Invoke("RestoreMuteState", 1f);
            }
        }
        else
        {
            Debug.LogError("TEST: Music source is null!");
        }
    }

    private void RestoreMuteState()
    {
        if (musicSource != null)
        {
            musicSource.mute = !isMusicEnabled;
        }
    }
}