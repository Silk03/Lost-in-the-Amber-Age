using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    
    [Header("Audio Settings")]
    // Replace AudioMixer with individual AudioSources
    public AudioSource[] gameSounds; // Drag all audio sources here
    public Toggle soundToggle;
    
    [Header("Animation")]
    public Animator menuAnimator; // Optional: if you want animation transitions
    
    [Header("Settings")]
    public string firstLevelName = "Level1"; // The scene to load when pressing Play
    
    [Header("UI Elements")]
    public Text soundStateText; // Text to show current sound state
    
    // Keep track of audio settings
    private bool soundOn = true;
    private MusicManager musicManager;
    
    private void Awake()
    {
        // Find or create music manager
        musicManager = FindObjectOfType<MusicManager>();
        if (musicManager == null)
        {
            GameObject managerObject = new GameObject("MusicManager");
            musicManager = managerObject.AddComponent<MusicManager>();
        }
    }

    private void Start()
    {
        // Make sure we start with the main menu active
        ShowMainMenu();
        
        // Load saved audio settings
        LoadAudioSettings();
    }
    
    // Show the main menu panel
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    // Show the settings panel
    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    // Start the game
    public void PlayGame()
    {
        // Optional: Add transition animation before loading
        if (menuAnimator != null)
        {
            menuAnimator.SetTrigger("StartGame");
            // Wait for animation before loading scene
            Invoke("LoadFirstLevel", 1f);
        }
        else
        {
            LoadFirstLevel();
        }
    }
    
    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(firstLevelName);
    }
    
    // Toggle sound on/off
    public void ToggleSound(bool isOn)
    {
        // Store the new sound state
        soundOn = isOn;
        Debug.Log("TOGGLE: Sound toggle changed to: " + isOn);
        
        // Always update the MusicManager first
        if (musicManager != null)
        {
            musicManager.SetMusicEnabled(soundOn);
            
            // Test sound to verify
            if (soundOn)
            {
                musicManager.TestSound();
            }
        }
        else
        {
            // Try to find it if it's null
            musicManager = FindObjectOfType<MusicManager>();
            if (musicManager != null)
            {
                musicManager.SetMusicEnabled(soundOn);
            }
            else
            {
                Debug.LogError("TOGGLE: No MusicManager found in scene!");
            }
        }
        
        // Update all other audio sources
        if (gameSounds != null)
        {
            foreach (AudioSource source in gameSounds)
            {
                if (source != null)
                {
                    source.mute = !soundOn;
                    Debug.Log("TOGGLE: Setting AudioSource mute: " + !soundOn);
                }
            }
        }
        
        // Save the setting
        PlayerPrefs.SetInt("SoundOn", soundOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("TOGGLE: Saved sound preference: " + soundOn);
    }
    
    public void ToggleSoundWithButton()
    {
        // Toggle the state
        soundOn = !soundOn;
        Debug.Log("Button toggle changed sound to: " + soundOn);
        
        // Update UI if we have a text display
        if (soundStateText != null)
        {
            soundStateText.text = soundOn ? "Sound: ON" : "Sound: OFF";
        }
        
        // Update toggle (without triggering its event)
        if (soundToggle != null)
        {
            // Temporarily remove listener
            Toggle.ToggleEvent cachedEvent = soundToggle.onValueChanged;
            soundToggle.onValueChanged = new Toggle.ToggleEvent();
            
            // Update state
            soundToggle.isOn = soundOn;
            
            // Restore listener
            soundToggle.onValueChanged = cachedEvent;
        }
        
        // Apply the sound change
        ApplySoundSettings();
    }
    
    // Extract actual sound application to a separate method
    private void ApplySoundSettings()
    {
        // Music manager
        if (musicManager != null)
        {
            musicManager.SetMusicEnabled(soundOn);
            
            // Test sound if enabled
            if (soundOn)
            {
                musicManager.TestSound();
            }
        }
        else
        {
            musicManager = FindObjectOfType<MusicManager>();
            if (musicManager != null)
            {
                musicManager.SetMusicEnabled(soundOn);
            }
        }
        
        // Game sounds
        if (gameSounds != null)
        {
            foreach (AudioSource source in gameSounds)
            {
                if (source != null)
                {
                    source.mute = !soundOn;
                }
            }
        }
        
        // Save setting
        PlayerPrefs.SetInt("SoundOn", soundOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    // Loads saved audio settings
    private void LoadAudioSettings()
    {
        // Default to sound on if no setting saved
        soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        
        // Update the toggle to match
        if (soundToggle != null)
        {
            // Set without triggering event
            bool originalValue = soundToggle.isOn;
            soundToggle.onValueChanged.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);
            soundToggle.isOn = soundOn;
            soundToggle.onValueChanged.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.RuntimeOnly);
        }
        
        // Update UI text
        if (soundStateText != null)
        {
            soundStateText.text = soundOn ? "Sound: ON" : "Sound: OFF";
        }
        
        // Apply the settings
        ApplySoundSettings();
    }
    
    // Quit the game
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
        // Stop play mode if in editor
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Quit the application if built
        Application.Quit();
        #endif
    }
}