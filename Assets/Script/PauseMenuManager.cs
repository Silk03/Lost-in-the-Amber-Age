using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    
    [Header("Audio")]
    public Toggle soundToggle;
    public Text soundStateText;
    
    // Track states
    private bool isPaused = false;
    private bool soundOn = true;
    private MusicManager musicManager;
    
    private void Start()
    {
        // Make sure panels are hidden at start
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        // Find music manager
        musicManager = FindObjectOfType<MusicManager>();
        
        // Load sound settings
        LoadSoundSettings();
    }
    
    private void Update()
    {
        // Optional: Allow ESC key to also pause/unpause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    // Toggle pause state on/off
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        // Show/hide the pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }
        
        // Hide settings panel if unpausing
        if (!isPaused && settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Freeze/unfreeze game
        Time.timeScale = isPaused ? 0f : 1f;
    }
    
    // Continue button
    public void ContinueGame()
    {
        TogglePause();
    }
    
    // Show settings panel
    public void ShowSettings()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
    
    // Back to pause menu from settings
    public void BackToPause()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    // Toggle sound on/off
    public void ToggleSound()
    {
        soundOn = !soundOn;
        
        // Update UI
        if (soundStateText != null)
        {
            soundStateText.text = soundOn ? "Sound: ON" : "Sound: OFF";
        }
        
        // Update toggle if available
        if (soundToggle != null)
        {
            soundToggle.isOn = soundOn;
        }
        
        // Apply sound settings
        ApplySoundSettings();
    }
    
    private void ApplySoundSettings()
    {
        if (musicManager != null)
        {
            musicManager.SetMusicEnabled(soundOn);
        }
        
        // Save setting
        PlayerPrefs.SetInt("SoundOn", soundOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void LoadSoundSettings()
    {
        soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        
        // Update toggle
        if (soundToggle != null)
        {
            soundToggle.isOn = soundOn;
        }
        
        // Update text
        if (soundStateText != null)
        {
            soundStateText.text = soundOn ? "Sound: ON" : "Sound: OFF";
        }
    }
    
    // Quit to main menu
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Make sure to reset time scale
        SceneManager.LoadScene("MainMenu");
    }
    
    // Quit game
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}