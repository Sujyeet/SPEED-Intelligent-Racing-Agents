using UnityEngine;
using UnityEditor;

namespace KartGame.Editor
{
    public class AISyncSetupTool : EditorWindow
    {
        [MenuItem("Tools/Setup Multiplayer AI Sync")]
        public static void SetupSync()
        {
            // Resolve types using reflection to bypass compilation constraints
            System.Type agentType = null;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "KartGame.AI")
                {
                    agentType = assembly.GetType("KartGame.AI.KartAgent");
                    if (agentType != null) break;
                }
            }

            if (agentType == null)
            {
                Debug.LogError("[AISyncSetupTool] Could not find KartAgent class type.");
                EditorUtility.DisplayDialog("AI Sync Setup Failed", "Could not find KartAgent class type in the project assemblies.", "OK");
                return;
            }

            System.Type netObjType = null;
            System.Type ownerAuthType = null;
            System.Type adapterType = typeof(KartGame.KartSystems.NetworkAISyncAdapter);

            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "Unity.Netcode.Runtime")
                {
                    netObjType = assembly.GetType("Unity.Netcode.NetworkObject");
                }
                if (assembly.GetName().Name == "KartGame")
                {
                    ownerAuthType = assembly.GetType("OwnerAuthoritativeKart");
                }
            }

            if (netObjType == null || ownerAuthType == null)
            {
                Debug.LogError("[AISyncSetupTool] Could not find Netcode NetworkObject or OwnerAuthoritativeKart types.");
                EditorUtility.DisplayDialog("AI Sync Setup Failed", "Could not find Netcode NetworkObject or OwnerAuthoritativeKart types.", "OK");
                return;
            }

            // Find all scene components matching KartAgent
            Object[] agents = Object.FindObjectsOfType(agentType);
            int sceneCount = 0;

            foreach (var agent in agents)
            {
                Component comp = agent as Component;
                if (comp == null) continue;

                GameObject go = comp.gameObject;
                bool changed = false;

                // 1. Add NetworkObject
                if (go.GetComponent(netObjType) == null)
                {
                    go.AddComponent(netObjType);
                    changed = true;
                    Debug.Log($"[AISyncSetupTool] Added NetworkObject to {go.name}");
                }

                // 2. Add OwnerAuthoritativeKart
                if (go.GetComponent(ownerAuthType) == null)
                {
                    go.AddComponent(ownerAuthType);
                    changed = true;
                    Debug.Log($"[AISyncSetupTool] Added OwnerAuthoritativeKart to {go.name}");
                }

                // 3. Add NetworkAISyncAdapter
                if (go.GetComponent(adapterType) == null)
                {
                    go.AddComponent(adapterType);
                    changed = true;
                    Debug.Log($"[AISyncSetupTool] Added NetworkAISyncAdapter to {go.name}");
                }

                if (changed)
                {
                    EditorUtility.SetDirty(go);
                    sceneCount++;
                }
            }

            if (sceneCount > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                Debug.Log($"[AISyncSetupTool] Configured sync components on {sceneCount} AI karts in the active scene. Please save the scene!");
                EditorUtility.DisplayDialog("AI Sync Setup Complete", $"Successfully configured sync components on {sceneCount} AI karts in the active scene.\n\nPlease save your scene (Ctrl+S) and play test!", "OK");
            }
            else
            {
                Debug.Log("[AISyncSetupTool] All AI karts in the active scene are already configured for multiplayer sync.");
                EditorUtility.DisplayDialog("AI Sync Setup", "All AI karts in the active scene are already configured for multiplayer sync.", "OK");
            }
        }
    }
}
