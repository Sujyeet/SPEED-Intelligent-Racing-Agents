using UnityEngine;
using UnityEditor;

public class BatchRename : EditorWindow
{
    private string baseName = "Waypoint_";
    private int startNumber = 0;
    private int increment = 1;

    [MenuItem("Tools/Batch Rename Waypoints")]
    public static void ShowWindow()
    {
        GetWindow<BatchRename>("Batch Rename Waypoints");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Rename Waypoints", EditorStyles.boldLabel);
        baseName = EditorGUILayout.TextField("Base Name", baseName);
        startNumber = EditorGUILayout.IntField("Start Number", startNumber);
        increment = EditorGUILayout.IntField("Increment", increment);

        if (GUILayout.Button("Rename Selected Waypoints"))
        {
            RenameSelectedWaypoints();
        }
    }

    private void RenameSelectedWaypoints()
    {
        int count = startNumber;
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj, "Rename Waypoint");
            obj.name = baseName + count.ToString();
            count += increment;
        }
        Debug.Log("Renamed " + Selection.gameObjects.Length + " waypoints.");
    }
}
