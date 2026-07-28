using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace CheckpointTools
{
    /// <summary>
    /// Unity Editor window for managing project checkpoints (save/load/list).
    /// Calls the PowerShell scripts in CheckpointTools/ folder.
    /// </summary>
    public class CheckpointWindow : EditorWindow
    {
        private Vector2 _scrollPos;
        private List<CheckpointInfo> _checkpoints = new List<CheckpointInfo>();
        private string _statusMessage = "";
        private MessageType _statusType = MessageType.Info;
        private bool _isBusy = false;
        private int _selectedIndex = -1;
        private string _newCheckpointLabel = "";
        private double _lastRefresh = 0;

        [MenuItem("Tools/Checkpoints/Checkpoint Manager %#c")] // Ctrl+Shift+C
        public static void ShowWindow()
        {
            var window = GetWindow<CheckpointWindow>("Checkpoints");
            window.minSize = new Vector2(420, 300);
            window.RefreshCheckpoints();
        }

        [MenuItem("Tools/Checkpoints/Save Checkpoint _F5")] // F5
        public static void QuickSave()
        {
            RunSaveCheckpoint("");
        }

        [MenuItem("Tools/Checkpoints/Load Latest Checkpoint _F6")] // F6
        public static void QuickLoadLatest()
        {
            RunLoadCheckpoint(0);
        }

        [MenuItem("Tools/Checkpoints/List Checkpoints _F7")] // F7
        public static void QuickList()
        {
            RunListCheckpoints();
        }

        private void OnEnable()
        {
            RefreshCheckpoints();
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            // Auto-refresh every 5 seconds if window is open
            if (EditorApplication.timeSinceStartup - _lastRefresh > 5)
            {
                RefreshCheckpoints();
            }
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawQuickActions();
            DrawCheckpointList();
            DrawStatusBar();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(5);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("📦 CHECKPOINT MANAGER", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("⟳ Refresh", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    RefreshCheckpoints();
                }
            }
            EditorGUILayout.Space(5);
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            using (new EditorGUILayout.HorizontalScope())
            {
                // Save button
                using (new EditorGUI.DisabledScope(_isBusy))
                {
                    if (GUILayout.Button("💾 Save Checkpoint", GUILayout.Height(30)))
                    {
                        SaveWithLabel();
                    }
                }

                // Label field for save
                _newCheckpointLabel = EditorGUILayout.TextField(_newCheckpointLabel, GUILayout.Width(180));
                EditorGUILayout.LabelField("(optional label)", EditorStyles.miniLabel, GUILayout.Width(90));
            }

            EditorGUILayout.Space(3);

            using (new EditorGUILayout.HorizontalScope())
            {
                // Load latest
                using (new EditorGUI.DisabledScope(_isBusy || _checkpoints.Count == 0))
                {
                    if (GUILayout.Button("📥 Load Latest", GUILayout.Height(26)))
                    {
                        LoadCheckpoint(0);
                    }
                }

                // Load selected
                using (new EditorGUI.DisabledScope(_isBusy || _selectedIndex < 0))
                {
                    if (GUILayout.Button("📥 Load Selected", GUILayout.Height(26)))
                    {
                        LoadCheckpoint(_selectedIndex);
                    }
                }

                // Delete selected
                using (new EditorGUI.DisabledScope(_isBusy || _selectedIndex < 0))
                {
                    if (GUILayout.Button("🗑 Delete", GUILayout.Height(26), GUILayout.Width(80)))
                    {
                        DeleteSelected();
                    }
                }
            }

            EditorGUILayout.Space(3);

            // Shortcuts hint
            EditorGUILayout.LabelField("Shortcuts:  F5 = Save   |   F6 = Load Latest   |   F7 = List", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawCheckpointList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Checkpoints ({_checkpoints.Count})", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Double-click to load", EditorStyles.miniLabel);
            }

            if (_checkpoints.Count == 0)
            {
                EditorGUILayout.HelpBox("No checkpoints yet. Click 'Save Checkpoint' to create your first one.", MessageType.Info);
            }
            else
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.MinHeight(150));

                for (int i = 0; i < _checkpoints.Count; i++)
                {
                    var cp = _checkpoints[i];
                    bool isSelected = (i == _selectedIndex);
                    bool isLatest = (i == 0);

                    using (new EditorGUILayout.HorizontalScope(isSelected ? "SelectionRect" : "Box"))
                    {
                        // Selection indicator
                        Color oldColor = GUI.backgroundColor;
                        if (isSelected) GUI.backgroundColor = new Color(0.3f, 0.6f, 1f, 1f);
                        else if (isLatest) GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f, 0.3f);

                        // Checkpoint info
                        EditorGUILayout.BeginVertical(GUILayout.Width(300));
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            string label = isLatest ? "👉 LATEST" : $"  #{i + 1}";
                            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Width(80));
                            
                            string displayName = cp.DisplayName;
                            if (!string.IsNullOrEmpty(cp.Label))
                                displayName += $"  <i>({cp.Label})</i>";
                            
                            EditorGUILayout.LabelField(displayName, new GUIStyle(EditorStyles.label) { richText = true, fontSize = 11 });
                        }

                        string timeStr = cp.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                        string ageStr = FormatAge(cp.Age);
                        EditorGUILayout.LabelField($"{timeStr}  ({ageStr})  •  {cp.SizeMB:F1} MB", EditorStyles.miniLabel);
                        EditorGUILayout.EndVertical();

                        GUILayout.FlexibleSpace();

                        // Load button on each row
                        using (new EditorGUI.DisabledScope(_isBusy))
                        {
                            if (GUILayout.Button("Load", GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                LoadCheckpoint(i);
                            }
                        }
                        
                        GUI.backgroundColor = oldColor;
                    }

                    // Handle double-click
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    Event e = Event.current;
                    if (e.type == EventType.MouseDown && e.clickCount == 2 && lastRect.Contains(e.mousePosition))
                    {
                        LoadCheckpoint(i);
                        e.Use();
                    }

                    // Handle single-click selection
                    if (e.type == EventType.MouseDown && e.clickCount == 1 && lastRect.Contains(e.mousePosition))
                    {
                        _selectedIndex = i;
                        Repaint();
                        e.Use();
                    }
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.Space(3);
            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.HelpBox(_statusMessage, _statusType);
            }
            else
            {
                EditorGUILayout.LabelField("Ready. Checkpoints stored in: Project/Checkpoints/", EditorStyles.miniLabel);
            }
        }

        private void RefreshCheckpoints()
        {
            _lastRefresh = EditorApplication.timeSinceStartup;
            _checkpoints.Clear();
            _selectedIndex = -1;

            string checkpointDir = GetCheckpointDirectory();
            if (!Directory.Exists(checkpointDir)) return;

            var dirs = new DirectoryInfo(checkpointDir).GetDirectories("checkpoint_*");
            foreach (var dir in dirs)
            {
                var files = dir.GetFiles("*", SearchOption.AllDirectories);
                long size = 0;
                foreach (var f in files) size += f.Length;

                string label = "";
                if (dir.Name.Contains("_") && dir.Name.Split('_').Length > 3)
                {
                    // Extract label from checkpoint_YYYYMMDD_HHMMSS_label
                    var parts = dir.Name.Split('_');
                    if (parts.Length > 3)
                    {
                        label = string.Join("_", parts, 3, parts.Length - 3).Replace('_', ' ');
                    }
                }

                _checkpoints.Add(new CheckpointInfo
                {
                    FullPath = dir.FullName,
                    Name = dir.Name,
                    DisplayName = dir.Name.Replace("checkpoint_", "").Replace('_', ' ').Substring(0, 15) + "...",
                    Label = label,
                    LastWriteTime = dir.LastWriteTime,
                    Age = System.DateTime.Now - dir.LastWriteTime,
                    SizeMB = size / (1024f * 1024f)
                });
            }

            // Sort newest first
            _checkpoints.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));
        }

        private void SaveWithLabel()
        {
            if (_isBusy) return;
            _isBusy = true;
            SetStatus("Saving checkpoint...", MessageType.Info);

            string label = _newCheckpointLabel.Trim().Replace(' ', '_');
            RunPowerShellAsync("Save-Checkpoint.ps1", label, (success, output) =>
            {
                _isBusy = false;
                _newCheckpointLabel = "";
                if (success)
                {
                    SetStatus($"✅ Checkpoint saved: {output.Trim()}", MessageType.Info);
                    RefreshCheckpoints();
                }
                else
                {
                    SetStatus($"❌ Save failed: {output}", MessageType.Error);
                }
            });
        }

        private void LoadCheckpoint(int index)
        {
            if (_isBusy || index < 0 || index >= _checkpoints.Count) return;

            var cp = _checkpoints[index];
            
            if (!EditorUtility.DisplayDialog("Confirm Restore",
                $"Restore checkpoint:\n\n{cp.Name}\n{cp.LastWriteTime}\n{cp.SizeMB:F1} MB\n\n" +
                "⚠️ This will REPLACE your current Assets/ and ProjectSettings/.\n" +
                "A safety backup will be created automatically.\n\n" +
                "Type 'YES' in the next dialog to confirm.",
                "YES - Restore", "Cancel"))
            {
                return;
            }

            _isBusy = true;
            SetStatus($"Restoring checkpoint {index + 1}...", MessageType.Warning);

            RunPowerShellAsync("Load-Checkpoint.ps1", $"-Index {index} -Force", (success, output) =>
            {
                _isBusy = false;
                if (success)
                {
                    SetStatus($"✅ Restored: {cp.Name}\nUnity will reimport assets. If issues: Delete Library/ folder and reopen.", MessageType.Info);
                    RefreshCheckpoints();
                }
                else
                {
                    SetStatus($"❌ Restore failed: {output}", MessageType.Error);
                }
            });
        }

        private void DeleteSelected()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _checkpoints.Count) return;

            var cp = _checkpoints[_selectedIndex];
            if (EditorUtility.DisplayDialog("Delete Checkpoint",
                $"Delete checkpoint:\n{cp.Name}\n{cp.SizeMB:F1} MB\n\nThis cannot be undone.",
                "Delete", "Cancel"))
            {
                try
                {
                    Directory.Delete(cp.FullPath, true);
                    SetStatus($"🗑 Deleted: {cp.Name}", MessageType.Info);
                    RefreshCheckpoints();
                }
                catch (System.Exception e)
                {
                    SetStatus($"❌ Delete failed: {e.Message}", MessageType.Error);
                }
            }
        }

        private void SetStatus(string msg, MessageType type)
        {
            _statusMessage = msg;
            _statusType = type;
        }

        private string FormatAge(System.TimeSpan age)
        {
            if (age.TotalMinutes < 1) return "just now";
            if (age.TotalHours < 1) return $"{(int)age.TotalMinutes}m ago";
            if (age.TotalDays < 1) return $"{(int)age.TotalHours}h ago";
            return $"{(int)age.TotalDays}d ago";
        }

        private string GetCheckpointDirectory()
        {
            return Path.Combine(Application.dataPath, "..", "Checkpoints");
        }

        private string GetToolsDirectory()
        {
            return Path.Combine(Application.dataPath, "..", "CheckpointTools");
        }

        // Static methods for menu shortcuts
        private static void RunSaveCheckpoint(string label)
        {
            RunPowerShell("Save-Checkpoint.ps1", label, (s, o) => {
                if (s) Debug.Log($"[Checkpoint] Saved: {o}");
                else Debug.LogError($"[Checkpoint] Save failed: {o}");
            });
        }

        private static void RunLoadCheckpoint(int index)
        {
            RunPowerShell("Load-Checkpoint.ps1", $"-Index {index} -Force", (s, o) => {
                if (s) Debug.Log($"[Checkpoint] Loaded: {o}");
                else Debug.LogError($"[Checkpoint] Load failed: {o}");
            });
        }

        private static void RunListCheckpoints()
        {
            RunPowerShell("List-Checkpoints.ps1", "", (s, o) => {
                Debug.Log($"[Checkpoint] List:\n{o}");
            });
        }

        private void RunPowerShellAsync(string scriptName, string args, System.Action<bool, string> callback)
        {
            EditorApplication.delayCall += () => {
                RunPowerShell(scriptName, args, callback);
            };
        }

        private static void RunPowerShell(string scriptName, string args, System.Action<bool, string> callback)
        {
            string toolsDir = Path.Combine(Directory.GetCurrentDirectory(), "CheckpointTools");
            string scriptPath = Path.Combine(toolsDir, scriptName);

            if (!File.Exists(scriptPath))
            {
                callback?.Invoke(false, $"Script not found: {scriptPath}");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" {args}",
                WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            try
            {
                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    bool success = process.ExitCode == 0;
                    string result = success ? output : (error + "\n" + output);
                    callback?.Invoke(success, result.Trim());
                }
            }
            catch (System.Exception e)
            {
                callback?.Invoke(false, e.Message);
            }
        }
    }

    public class CheckpointInfo
    {
        public string FullPath;
        public string Name;
        public string DisplayName;
        public string Label;
        public System.DateTime LastWriteTime;
        public System.TimeSpan Age;
        public float SizeMB;
    }
}