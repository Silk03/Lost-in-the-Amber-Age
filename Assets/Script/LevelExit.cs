using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelExit : MonoBehaviour
{
    // Set this to the name of your Level 2 scene in the Inspector
    public string nextLevelName = "Level2";
    
    // Optional: delay before loading next level
    public float loadDelay = 1f;
    public float fadeDuration = 1f;
    
    private bool isLoading = false;
    private float fadeAmount = 0f;
    private Texture2D blackTexture;
    
    void Start()
    {
        // Create the black texture once at the start
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }
    
    // Called when another collider enters this trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if it's the player and we're not already loading
        if (collision.CompareTag("Player") && !isLoading)
        {
            Debug.Log("Player reached the exit! Loading " + nextLevelName);
            
            // Save progress (optional)
            PlayerPrefs.SetInt("CurrentLevel", 2);
            PlayerPrefs.Save();
            
            // Start fade and load next level
            StartCoroutine(FadeAndLoadLevel());
            isLoading = true;
        }
    }
    
    // This is where GUI rendering happens
    private void OnGUI()
    {
        // Only draw if we're in the process of fading
        if (fadeAmount > 0)
        {
            // Set the color with the current fade amount
            Color guiColor = GUI.color;
            GUI.color = new Color(0, 0, 0, fadeAmount);
            
            // Draw the texture to cover the whole screen
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
            
            // Reset GUI color
            GUI.color = guiColor;
        }
    }
    
    IEnumerator FadeAndLoadLevel()
    {
        // Gradually increase the fade amount
        float elapsedTime = 0;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeAmount = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }
        
        // Make sure we're fully faded before loading
        fadeAmount = 1.0f;
        
        // Optional delay before loading
        yield return new WaitForSeconds(loadDelay);
        
        // Load the next level
        SceneManager.LoadScene(nextLevelName);
    }
}