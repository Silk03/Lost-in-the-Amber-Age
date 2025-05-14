using UnityEngine;

public class GameManager1 : MonoBehaviour
{
    public static GameManager1 Instance { get; private set; }
    
    public GameObject pauseMenuPrefab;
    private GameObject pauseMenuInstance;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Instantiate the pause menu
            if (pauseMenuPrefab != null)
            {
                pauseMenuInstance = Instantiate(pauseMenuPrefab);
                DontDestroyOnLoad(pauseMenuInstance);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // This can be called from other scripts to trigger the pause menu
    public void TogglePause()
    {
        if (pauseMenuInstance != null)
        {
            PauseMenuManager pauseManager = pauseMenuInstance.GetComponent<PauseMenuManager>();
            if (pauseManager != null)
            {
                if (Time.timeScale == 0) // Already paused
                    pauseManager.ResumeGame();
                else
                    pauseManager.PauseGame();
            }
        }
    }
}