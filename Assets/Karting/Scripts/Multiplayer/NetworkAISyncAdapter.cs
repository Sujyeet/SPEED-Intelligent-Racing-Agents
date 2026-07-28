using UnityEngine;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    [DefaultExecutionOrder(-100)]
    public class NetworkAISyncAdapter : MonoBehaviour
    {
        private void Awake()
        {
            // Register callback for network startup
            NetworkManager netManager = NetworkManager.Singleton;
            if (netManager != null)
            {
                netManager.OnClientStarted += OnClientStarted;
            }
        }

        private void Start()
        {
            // Immediate check in case network was already started
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
            {
                EvaluateAISync();
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
            }
        }

        private void OnClientStarted()
        {
            EvaluateAISync();
        }

        private void EvaluateAISync()
        {
            // If we are a client (not the server/host), we must disable the local AI decision-making scripts
            // and local physics calculation so we don't fight position sync updates from the host.
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            {
                DisableAI();
            }
            else
            {
                EnableAI();
            }
        }

        private void DisableAI()
        {
            // Disable the decision making (use string to bypass assembly boundary constraints)
            Behaviour agent = GetComponent("KartAgent") as Behaviour;
            if (agent != null) 
            {
                agent.enabled = false;
                Debug.Log($"[NetworkAISyncAdapter] Disabled KartAgent on {gameObject.name} (Client Mode)");
            }

            Behaviour decisionRequester = GetComponent("DecisionRequester") as Behaviour;
            if (decisionRequester != null)
            {
                decisionRequester.enabled = false;
                Debug.Log($"[NetworkAISyncAdapter] Disabled DecisionRequester on {gameObject.name} (Client Mode)");
            }

            // Disable player inputs if any
            var keyboardInput = GetComponent<KeyboardInput>();
            if (keyboardInput != null) keyboardInput.enabled = false;

            // Set rigidbody to kinematic so clients only interpolate position
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }

        private void EnableAI()
        {
            // Host/Server should run the AI
            Behaviour agent = GetComponent("KartAgent") as Behaviour;
            if (agent != null) agent.enabled = true;

            Behaviour decisionRequester = GetComponent("DecisionRequester") as Behaviour;
            if (decisionRequester != null) decisionRequester.enabled = true;

            var rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
        }
    }
}
