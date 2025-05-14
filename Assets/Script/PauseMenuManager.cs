using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    
    [Header("Settings")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;
    
    [Header("Audio")]
    public AudioClip buttonSound;
    
    private bool isPaused = false;
    private AudioSource audioSource;
    
    void Start()
    {
        // Set initial panel states
        pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        // Initialize audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && buttonSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Initialize settings values
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
    }
    
    void Update()
    {
        // Toggle pause menu with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                PauseGame();
            else if (settingsPanel.activeSelf)
                BackToMainPause();
            else
                ResumeGame();
        }
    }
    
    public void PauseGame()
    {
        PlayButtonSound();
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Freeze the game
        isPaused = true;
    }
    
    public void ResumeGame()
    {
        PlayButtonSound();
        pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        Time.timeScale = 1f; // Resume normal time
        isPaused = false;
    }
    
    public void OpenSettings()
    {
        PlayButtonSound();
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    public void BackToMainPause()
    {
        PlayButtonSound();
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
    
    public void ReturnToMainMenu()
    {
        PlayButtonSound();
        Time.timeScale = 1f; // Reset time scale
        SceneManager.LoadScene("MainMenu");
    }
    
    // Settings methods
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("GameVolume", volume);
        PlayerPrefs.Save();
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonSound != null)
            audioSource.PlayOneShot(buttonSound);
    }
}