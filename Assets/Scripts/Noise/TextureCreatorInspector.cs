using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NoiseGenerator))]
public class TextureCreatorInspector : Editor
{
    private NoiseGenerator creator;

    private void OnEnable()
    {
        creator = target as NoiseGenerator;
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
