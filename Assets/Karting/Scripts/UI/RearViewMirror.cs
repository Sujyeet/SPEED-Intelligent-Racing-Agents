using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    public class RearViewMirror : NetworkBehaviour
    {
        [Header("Rear Camera")]
        public Camera rearCamera;

        [Header("Toggle Key")]
        public KeyCode toggleKey = KeyCode.V;

        private RawImage mirrorDisplay;
        private bool mirrorEnabled = false;

        public override void OnNetworkSpawn()
        {
            mirrorDisplay = FindMirrorDisplay();

            if (!IsOwner)
            {
                if (rearCamera != null)
                    rearCamera.gameObject.SetActive(false);

                if (mirrorDisplay != null)
                    mirrorDisplay.gameObject.SetActive(false);

                return;
            }

            SetMirror(false);
        }

        void Update()
        {
            if (!IsOwner)
                return;

            if (Input.GetKeyDown(toggleKey))
            {
                mirrorEnabled = !mirrorEnabled;
                SetMirror(mirrorEnabled);
            }
        }

        private RawImage FindMirrorDisplay()
        {
            RawImage[] rawImages = FindObjectsOfType<RawImage>(true);

            foreach (RawImage img in rawImages)
            {
                if (img.name == "RearViewMirror")
                    return img;
            }

            Debug.LogWarning("[RearViewMirror] Could not find RawImage named 'RearViewMirror' in scene.");
            return null;
        }

        private void SetMirror(bool enabled)
        {
            if (rearCamera != null)
                rearCamera.gameObject.SetActive(enabled);

            if (mirrorDisplay != null)
                mirrorDisplay.gameObject.SetActive(enabled);
        }
    }
}