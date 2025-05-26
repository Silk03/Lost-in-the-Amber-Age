using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseButton;        // Button always visible during gameplay
    public GameObject pauseMenuPanel;     // Main pause menu
    public GameObject settingsPanel;      // Settings submenu
    
    [Header("Sound Settings")]
    public Button soundToggleButton;      // Button to toggle sound
    public Text soundButtonText;          // Text showing ON/OFF state
    
    private bool isPaused = false;
    private bool isSoundOn = true;
    private MusicManager musicManager;
    
    private void Start()
    {
        // Make sure panels start hidden
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        // Get music manager
        musicManager = FindObjectOfType<MusicManager>();
        
        // Load sound settings
        LoadSoundSettings();
    }
    
    // Called when pause button is pressed
    public void PauseGame()
    {
        isPaused = true;
        
        // Show pause menu
        if (pauseButton != null)
            pauseButton.SetActive(false);
            
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
            
        // Freeze game
        Time.timeScale = 0f;
    }
    
    // Called when Continue button is pressed
    public void ContinueGame()
    {
        isPaused = false;
        
        // Hide pause menu
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (pauseButton != null)
            pauseButton.SetActive(true);
            
        // Unfreeze game
        Time.timeScale = 1f;
    }
    
    // Called when Settings button is pressed
    public void OpenSettings()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
    
    // Called when Back button in settings is pressed
    public void CloseSettings()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    // Toggle sound on/off
    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        
        // Update text
        UpdateSoundButtonText();
        
        // Apply sound settings
        ApplySoundSettings();
    }
    
    private void UpdateSoundButtonText()
    {
        if (soundButtonText != null)
        {
            soundButtonText.text = "Sound: " + (isSoundOn ? "ON" : "OFF");
        }
    }
    
    private void ApplySoundSettings()
    {
        // Apply to music manager
        if (musicManager != null)
        {
            musicManager.SetMusicEnabled(isSoundOn);
        }
        
        // Save setting
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void LoadSoundSettings()
    {
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        UpdateSoundButtonText();
    }
    
    // Return to main menu
    public void ReturnToMainMenu()
    {
        // Unfreeze time
        Time.timeScale = 1f;
        
        // Load main menu scene
        SceneManager.LoadScene("MainMenu");
    }
    
    // Exit game
    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}