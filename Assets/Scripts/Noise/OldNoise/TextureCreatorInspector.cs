using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OldNoiseGenerator))]
public class TextureCreatorInspector : Editor
{
    private OldNoiseGenerator creator;

    private void OnEnable()
    {
        creator = target as OldNoiseGenerator;
        Undo.undoRedoPerformed += RefreshCreator;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= RefreshCreator;
    }

    private void RefreshCreator()
    {
        if (Application.isPlaying)
        {
            creator.FillTexture();
            creator.BlurImage();
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            RefreshCreator();
        }
    }
}
