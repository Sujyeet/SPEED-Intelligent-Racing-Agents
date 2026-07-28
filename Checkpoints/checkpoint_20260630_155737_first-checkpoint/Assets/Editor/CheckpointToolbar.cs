using UnityEngine;
using UnityEditor;
using System.Diagnostics;

[InitializeOnLoad]
public static class CheckpointToolbar
{
    static CheckpointToolbar()
    {
        ToolbarExtender.ToolbarExtender.OnToolbarGUI += OnToolbarGUI;
    }

    static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace();

        // Style for toolbar buttons
        var style = new GUIStyle(EditorStyles.toolbarButton)
        {
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(10, 10, 2, 2)
        };

        // SAVE CHECKPOINT button
        if (GUILayout.Button("💾 Save Checkpoint", style, GUILayout.Width(130)))
        {
            SaveCheckpointQuick();
        }

        // LOAD CHECKPOINT button
        if (GUILayout.Button("🔄 Load Checkpoint", style, GUILayout.Width(130)))
        {
            LoadCheckpointQuick();
        }

        // LIST CHECKPOINTS button
        if (GUILayout.Button("📋 List", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            ListCheckpointsQuick();
        }

        GUILayout.Space(10);
    }

    [MenuItem("Tools/Checkpoints/Save Checkpoint _F2")]
    public static void SaveCheckpointQuick()
    {
        RunCheckpointTool("Save-Checkpoint.ps1", "");
    }

    [MenuItem("Tools/Checkpoints/Load Checkpoint _F3")]
    public static void LoadCheckpointQuick()
    {
        // Show quick save dialog first
        if (EditorUtility.DisplayDialog("Load Checkpoint", 
            "This will restore Assets/ and ProjectSettings/ from a checkpoint.\n\n" +
            "⚠️ CLOSE UNITY FIRST for best results.\n\n" +
            "A safety backup of current state will be created automatically.", 
            "Continue", "Cancel"))
        {
            RunCheckpointTool("Load-Checkpoint.ps1", "");
        }
    }

    [MenuItem("Tools/Checkpoints/List Checkpoints")]
    public static void ListCheckpointsQuick()
    {
        RunCheckpointTool("List-Checkpoints.ps1", "-Size");
    }

    [MenuItem("Tools/Checkpoints/Open Checkpoints Folder")]
    public static void OpenCheckpointsFolder()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "..", "Checkpoints");
        if (System.IO.Directory.Exists(path))
        {
            EditorUtility.RevealInFinder(path);
        }
        else
        {
            EditorUtility.DisplayDialog("Checkpoints", "No Checkpoints folder yet.\nCreate a checkpoint first!", "OK");
        }
    }

    [MenuItem("Tools/Checkpoints/Settings")]
    public static void CheckpointSettings()
    {
        // Open the PowerShell script folder
        string path = System.IO.Path.Combine(Application.dataPath, "..", "CheckpointTools");
        EditorUtility.RevealInFinder(path);
    }

    static void RunCheckpointTool(string scriptName, string args)
    {
        string projectRoot = System.IO.Path.Combine(Application.dataPath, "..");
        string scriptPath = System.IO.Path.Combine(projectRoot, "CheckpointTools", scriptName);

        if (!System.IO.File.Exists(scriptPath))
        {
            EditorUtility.DisplayDialog("Error", 
                $"Checkpoint script not found:\n{scriptPath}\n\n" +
                "Make sure CheckpointTools folder exists in project root.", "OK");
            return;
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" {args}",
                WorkingDirectory = projectRoot,
                UseShellExecute = false,
                CreateNoWindow = false, // Show window for interactive scripts
                WindowStyle = ProcessWindowStyle.Normal
            }
        };

        try
        {
            process.Start();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to start checkpoint tool:\n{e.Message}", "OK");
        }
    }
}

// Toolbar extender from Unity community (MIT license)
public static class ToolbarExtender
{
    public static event System.Action OnToolbarGUI;

    [InitializeOnLoadMethod]
    static void Init()
    {
        // For Unity 2021.2+
        #if UNITY_2021_2_OR_NEWER
        UnityEditor.Toolbar.toolbarGUI += () => OnToolbarGUI?.Invoke();
        #else
        // For older Unity versions
        System.Type toolbarType = System.Type.GetType("UnityEditor.Toolbar,UnityEditor");
        if (toolbarType != null)
        {
            var field = toolbarType.GetField("toolbarGUI", 
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                var delegates = field.GetValue(null) as System.Delegate;
                if (delegates != null)
                {
                    var newDelegate = System.Delegate.Combine(delegates, 
                        new System.Action(() => OnToolbarGUI?.Invoke()));
                    field.SetValue(null, newDelegate);
                }
            }
        }
        #endif
    }
}