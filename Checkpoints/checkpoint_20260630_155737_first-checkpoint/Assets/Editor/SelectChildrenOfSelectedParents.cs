using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SelectChildrenOfSelectedParents : MonoBehaviour
{
    [MenuItem("Tools/Select Children of Selected Parents %#c")] // Ctrl/Cmd + Shift + C shortcut
    private static void SelectChildren()
    {
        GameObject[] selectedParents = Selection.gameObjects;
        List<GameObject> childrenToSelect = new List<GameObject>();

        foreach (GameObject parent in selectedParents)
        {
            foreach (Transform child in parent.transform)
            {
                childrenToSelect.Add(child.gameObject);
            }
        }

        if (childrenToSelect.Count > 0)
        {
            Selection.objects = childrenToSelect.ToArray();
            Debug.Log($"Selected {childrenToSelect.Count} children of {selectedParents.Length} parents.");
        }
        else
        {
            Debug.Log("No children found to select.");
        }
    }
}
