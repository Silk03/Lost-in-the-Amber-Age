// Create this new script: InfoPopup.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PteroInfo : MonoBehaviour 
{
    public static PteroInfo Instance { get; private set; }
    
    public GameObject popupPanel;
    public TMP_Text descriptionText;
    public Image enemyImage;
    public TMP_Text enemyNameText;
    
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
        }
        
        // Ensure panel starts hidden
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
    
    public void ShowEnemyInfo(string name, string description, Sprite icon) 
    {
        Debug.Log($"Showing info for: {name}");
        
        if (popupPanel != null) 
        {
            popupPanel.SetActive(true);
            
            if (descriptionText != null)
                descriptionText.text = description;
                
            if (enemyNameText != null)
                enemyNameText.text = name;
                
            if (enemyImage != null && icon != null)
                enemyImage.sprite = icon;
                
            StartCoroutine(HideAfterDelay(3.0f));
        } 
        else 
        {
            Debug.LogError("Popup panel not assigned in InfoPopup!");
        }
    }
    
    private IEnumerator HideAfterDelay(float delay) 
    {
        yield return new WaitForSeconds(delay);
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}