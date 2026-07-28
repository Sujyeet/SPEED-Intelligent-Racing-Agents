using UnityEngine;
using Unity.Netcode;

namespace KartGame.KartSystems 
{
    public class KeyboardInput : BaseInput
    {
        public string TurnInputName = "Horizontal";
        public string AccelerateButtonName = "Accelerate";
        public string BrakeButtonName = "Brake";

        private NetworkBehaviour networkBehaviour;

        private void Awake()
        {
            // Get the NetworkBehaviour component (NetworkedArcadeKart)
            networkBehaviour = GetComponent<NetworkBehaviour>();
        }

        public override InputData GenerateInput() 
        {
            // Check if this is a networked kart
            if (networkBehaviour != null)
            {
                // Only read input if WE OWN this kart
                if (!networkBehaviour.IsOwner)
                {
                    // This is someone else's kart - return empty input
                    return new InputData
                    {
                        Accelerate = false,
                        Brake = false,
                        TurnInput = 0f
                    };
                }
            }

            // We own this kart - read keyboard input
            bool accelerate = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            bool brake = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            float turn = Input.GetAxis(TurnInputName);

            // Also try the configured button names (if they exist in Input Manager)
            try
            {
                if (Input.GetButton(AccelerateButtonName))
                    accelerate = true;
                if (Input.GetButton(BrakeButtonName))
                    brake = true;
            }
            catch
            {
                // Buttons not configured - that's okay, we're using W/S keys
            }

            return new InputData
            {
                Accelerate = accelerate,
                Brake = brake,
                TurnInput = turn
            };
        }
    }
}
