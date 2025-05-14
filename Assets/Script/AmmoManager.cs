using UnityEngine;
using TMPro;

public class AmmoManager : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int maxAmmo = 20;
    public int currentAmmo = 10;
    public int ammoPerPickup = 5;
    
    [Header("UI")]
    public TextMeshProUGUI ammoText; // To display ammo count
    
    [Header("Effects")]
    public AudioClip shootSound;
    public AudioClip emptySound;
    public AudioClip pickupSound;
    
    private AudioSource audioSource;
    
    private void Start()
    {
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Initialize UI
        UpdateAmmoUI();
    }
    
    // Call this to use ammo when shooting
    public bool UseAmmo()
    {
        if (currentAmmo <= 0)
        {
            // No ammo left!
            if (emptySound != null && audioSource != null)
                audioSource.PlayOneShot(emptySound);
            
            Debug.Log("Out of ammo!");
            return false;
        }
        
        // Decrease ammo and update UI
        currentAmmo--;
        UpdateAmmoUI();
        
        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound);
            
        return true;
    }
    
    // Call this when collecting ammo pickup
    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        
        // Cap at max ammo
        if (currentAmmo > maxAmmo)
            currentAmmo = maxAmmo;
            
        UpdateAmmoUI();
        
        if (pickupSound != null && audioSource != null)
            audioSource.PlayOneShot(pickupSound);
            
        Debug.Log("Picked up ammo: " + amount + ". Total: " + currentAmmo);
    }
    
    // Update the UI text
    private void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = "Ammo: " + currentAmmo;
    }
}