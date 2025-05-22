using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GamePopupManager : MonoBehaviour
{
    public static GamePopupManager Instance { get; private set; }
    
    [Header("Popup Panels")]
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    
    [Header("Game Over Elements")]
    public Image gameOverImage;
    public TMP_Text gameOverText;
    public TMP_Text deathReasonText;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("Level Complete Elements")]
    public Image victoryImage;
    public TMP_Text levelCompleteText;
    public TMP_Text statsText;
    public Button nextLevelButton;
    public Button continueButton;
    
    [Header("Animation Settings")]
    public float popupDelay = 0.5f;
    public float fadeInDuration = 0.5f;
    
    private CanvasGroup gameOverCanvasGroup;
    private CanvasGroup levelCompleteCanvasGroup;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Get canvas groups or add them
        if (gameOverPanel != null)
        {
            gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (gameOverCanvasGroup == null)
                gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
                
            gameOverPanel.SetActive(false);
        }
        
        if (levelCompletePanel != null)
        {
            levelCompleteCanvasGroup = levelCompletePanel.GetComponent<CanvasGroup>();
            if (levelCompleteCanvasGroup == null)
                levelCompleteCanvasGroup = levelCompletePanel.AddComponent<CanvasGroup>();
                
            levelCompletePanel.SetActive(false);
        }
        
        // Set up button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(GoToNextLevel);
            
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);
    }
    
    public void ShowGameOver(string reasonForDeath = "You died!")
    {
        StartCoroutine(DisplayGameOver(reasonForDeath));
    }
    
    public void ShowLevelComplete(string stats = "")
    {
        StartCoroutine(DisplayLevelComplete(stats));
    }
    
    private IEnumerator DisplayGameOver(string reasonForDeath)
    {
        // Wait a moment before showing popup
        yield return new WaitForSeconds(popupDelay);
        
        // Setup panel
        gameOverPanel.SetActive(true);
        gameOverCanvasGroup.alpha = 0;
        
        // Set text
        if (deathReasonText != null)
            deathReasonText.text = reasonForDeath;
        
        // Fade in
        float elapsedTime = 0;
        while (elapsedTime < fadeInDuration)
        {
            gameOverCanvasGroup.alpha = elapsedTime / fadeInDuration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        gameOverCanvasGroup.alpha = 1;
        
        // Play death sound if you have one
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
            audioSource.Play();
    }
    
    private IEnumerator DisplayLevelComplete(string stats)
    {
        // Wait a moment before showing popup
        yield return new WaitForSeconds(popupDelay);
        
        // Setup panel
        levelCompletePanel.SetActive(true);
        levelCompleteCanvasGroup.alpha = 0;
        
        // Set stats text
        if (statsText != null)
            statsText.text = stats;
        
        // Fade in
        float elapsedTime = 0;
        while (elapsedTime < fadeInDuration)
        {
            levelCompleteCanvasGroup.alpha = elapsedTime / fadeInDuration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        levelCompleteCanvasGroup.alpha = 1;
        
        // Play victory sound if you have one
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
            audioSource.Play();
    }
    
    // Button event handlers
    public void RestartLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }
    
    public void ReturnToMainMenu()
    {
        // Change 0 to your main menu scene index
        SceneManager.LoadScene(0);
    }
    
    public void GoToNextLevel()
    {
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextScene < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextScene);
        else
            Debug.LogWarning("No next level available!");
    }
    
    public void ContinueGame()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }
}