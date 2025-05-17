using UnityEngine;
using UnityEditor;

public class EmptyEditorScript : Editor
{
    [MenuItem("Tools/Empty Tool")]
    public static void EmptyTool()
    {
        Debug.Log("Empty Editor Tool");
    }
}