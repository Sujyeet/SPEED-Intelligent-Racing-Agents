using UnityEngine;
using UnityEngine.UI;

namespace KartGame.UI
{
    /// <summary>
    /// Drop this on ANY object in the MainScene (e.g. GameHUD or GameModeManager).
    /// It automatically fixes the Canvas sizes for high-resolution monitors (like 1600p)
    /// and draws the Relay Join Code in the top-left corner!
    /// </summary>
    public class HUDPolishFix : MonoBehaviour
    {
        private void Awake()
        {
            // 1. FIX THE CANVAS SCALING FOR 1440p / 1600p / 4K MONITORS
            CanvasScaler[] scalers = FindObjectsOfType<CanvasScaler>(true);
            foreach (var scaler in scalers)
            {
                // Force it to scale dynamically with the screen size
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                
                // Use a standard 1080p baseline reference
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                // Blend halfway between stretching width vs height
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
            
            Debug.Log($"[HUDPolishFix] Automatically optimized {scalers.Length} Canvas Scalers for high-resolution monitors!");
        }

        private void OnGUI()
        {
            // 2. DISPLAY THE RELAY JOIN CODE ON SCREEN
            if (!string.IsNullOrEmpty(RelayManager.JoinCode))
            {
                // Make the text big and easy to read
                GUIStyle style = new GUIStyle();
                style.fontSize = 30;
                style.fontStyle = FontStyle.Bold;
                
                // Draw a black outline for readability
                style.normal.textColor = Color.black;
                GUI.Label(new Rect(22, 22, 400, 50), $"Room Code: {RelayManager.JoinCode}", style);
                GUI.Label(new Rect(18, 18, 400, 50), $"Room Code: {RelayManager.JoinCode}", style);
                GUI.Label(new Rect(22, 18, 400, 50), $"Room Code: {RelayManager.JoinCode}", style);
                GUI.Label(new Rect(18, 22, 400, 50), $"Room Code: {RelayManager.JoinCode}", style);
                
                // Draw white text
                style.normal.textColor = Color.white;
                GUI.Label(new Rect(20, 20, 400, 50), $"Room Code: {RelayManager.JoinCode}", style);
            }
        }
    }
}
