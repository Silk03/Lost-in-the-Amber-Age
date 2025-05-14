using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class InfoPopup : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyInfoText;
    public Image enemyIcon;
    
    [Header("Settings")]
    public float displayTime = 3f;
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.5f;
    
    private CanvasGroup canvasGroup;
    private Coroutine activePopupCoroutine;
    
    // Singleton pattern for easy access
    public static InfoPopup Instance { get; private set; }
    
    void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep it between scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Get the canvas group
        canvasGroup = popupPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = popupPanel.AddComponent<CanvasGroup>();
        }
        
        // Hide popup initially
        popupPanel.SetActive(false);
    }
    
    public void ShowEnemyInfo(string enemyName, string enemyInfo, Sprite icon = null)
    {
        // Stop any active popup coroutine
        if (activePopupCoroutine != null)
        {
            StopCoroutine(activePopupCoroutine);
        }
        
        // Start new popup display
        activePopupCoroutine = StartCoroutine(DisplayPopup(enemyName, enemyInfo, icon));
    }
    
    private IEnumerator DisplayPopup(string enemyName, string enemyInfo, Sprite icon)
    {
        // Check for missing references
        if (enemyNameText == null || enemyInfoText == null || popupPanel == null)
        {
            Debug.LogError("UI Elements not assigned in InfoPopup!");
            yield break;
        }
        
        // Set up popup content
        enemyNameText.text = enemyName;
        enemyInfoText.text = enemyInfo;
        
        if (icon != null && enemyIcon != null)
        {
            enemyIcon.sprite = icon;
            enemyIcon.gameObject.SetActive(true);
        }
        else if (enemyIcon != null)
        {
            enemyIcon.gameObject.SetActive(false);
        }
        
        // Show the popup
        popupPanel.SetActive(true);
        canvasGroup.alpha = 0;
        
        // Fade in
        float timer = 0;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = timer / fadeInTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
        
        // Wait for display time
        yield return new WaitForSeconds(displayTime);
        
        // Fade out
        timer = 0;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = 1 - (timer / fadeOutTime);
            yield return null;
        }
        
        // Hide the popup
        popupPanel.SetActive(false);
        activePopupCoroutine = null;
    }
    
    // For testing in the editor
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // Press T to test
        {
            ShowEnemyInfo("Test Enemy", "This is a test description to verify the popup system works correctly.", null);
        }
    }
}