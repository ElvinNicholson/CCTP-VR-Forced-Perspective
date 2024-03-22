using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnamorphicDecalCamera))]
public class AnamorphicProjectionEditor : Editor
{
    private AnamorphicDecalCamera anamorphicDecalCamera;

    private void OnEnable()
    {
        anamorphicDecalCamera = (AnamorphicDecalCamera)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Project Decals"))
        {
            anamorphicDecalCamera.ProjectDecals();
        }
    }
}
