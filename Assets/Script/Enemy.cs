using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float hitpoints;
    public float maxhitpoints = 5;

    [Header("Info Popup Details")]
    public string enemyName = "Goblin"; // The name to display
    [TextArea(2, 5)]
    public string enemyDescription = "A weak but annoying creature that attacks in groups."; // Description to show
    public Sprite enemyIcon; // Optional: Enemy portrait

    public GameObject raptorInfoPanel; // Assign this in the Inspector
    public TMP_Text raptorInfoText;

    void Start()
    {
        hitpoints = maxhitpoints;
    }

    public void TakeHit(float damage)
    {
        hitpoints -= damage;
        if (hitpoints <= 0)
        {
            Debug.Log($"Enemy {enemyName} died, attempting to show popup");
            
            // Use only one popup system (InfoPopup recommended)
            if (InfoPopup.Instance != null)
            {
                InfoPopup.Instance.ShowEnemyInfo(enemyName, enemyDescription, enemyIcon);
            }
            else
            {
                Debug.LogError("InfoPopup.Instance is null! Make sure InfoPopup is in the scene");
            }
            
            Destroy(gameObject, 0.1f);
        }
    }

    void ShowRaptorInfo()
    {
        if (raptorInfoPanel != null && raptorInfoText != null)
        {
            raptorInfoPanel.SetActive(true);
            raptorInfoText.text = "Raptor Info:\n- Species: Velociraptor\n- Speed: Fast\n- Habitat: Prehistoric Forests";
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collided with: {collision.gameObject.name}");
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit! Triggering Game Over.");
            GameManager.Instance.GameOver();
        }
    }

}
