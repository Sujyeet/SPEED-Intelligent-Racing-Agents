using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField joinCodeInput;
    public TMP_Text joinCodeDisplay;
    
    private RelayManager relayManager;

    private void Start()
    {
        relayManager = GetComponent<RelayManager>();
        
        if (relayManager == null)
        {
            Debug.LogError("RelayManager not found! Add it to this GameObject.");
        }
    }

    public async void StartHost()
    {
        Debug.Log("Creating online relay...");
        
        string joinCode = await relayManager.CreateRelay();
        
        if (!string.IsNullOrEmpty(joinCode))
        {
            Debug.Log($"========================================");
            Debug.Log($"JOIN CODE: {joinCode}");
            Debug.Log($"Share this code with your friend!");
            Debug.Log($"========================================");
            
            // Show join code on screen
            if (joinCodeDisplay != null)
            {
                joinCodeDisplay.text = $"JOIN CODE: {joinCode}";
                joinCodeDisplay.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Failed to create relay!");
        }
    }
    
    public async void StartClient()
    {
        if (joinCodeInput == null || string.IsNullOrEmpty(joinCodeInput.text))
        {
            Debug.LogError("Please enter a join code!");
            return;
        }
        
        string code = joinCodeInput.text.Trim().ToUpper();
        Debug.Log($"Attempting to join with code: {code}");
        
        bool success = await relayManager.JoinRelay(code);
        
        if (success)
        {
            Debug.Log("Successfully joined the game!");
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Failed to join! Check the code and try again.");
        }
    }
}
