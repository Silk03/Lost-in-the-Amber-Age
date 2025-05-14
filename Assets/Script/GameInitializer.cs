using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    // This will be our entry point into the game
    void Start()
    {
        // Load the main menu
        SceneManager.LoadScene("MainMenu");
    }
}