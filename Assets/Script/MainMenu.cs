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
    
    // Keep track of audio settings
    private bool soundOn = true;
    
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
        soundOn = isOn;
        
        // Instead of using AudioMixer, directly control all audio sources
        foreach (AudioSource source in gameSounds)
        {
            if (source != null)
            {
                source.mute = !soundOn;
            }
        }
        
        // Save the setting
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
            soundToggle.isOn = soundOn;
        }
        
        // Apply the saved setting
        ToggleSound(soundOn);
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