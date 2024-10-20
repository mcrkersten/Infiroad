using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GuardrailSettings))]
public class GuardrailSettingsInspector : MeshtaskSettingsInspector
{
    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    void OnSceneGUI(SceneView sceneView)
    {
        EditorUtility.SetDirty(target);
        MeshtaskSettings t = target as GuardrailSettings;
        Undo.RecordObject(t, "GuardrailSettings");
        Draw3DShape(t);
        DrawHandles(t);
        t.EDITOR_offSetPosition = DrawOffsetHandle(t);
    }
}

[CustomEditor(typeof(GrandstandSettings))]
public class GrandstandSettingsInspector : MeshtaskSettingsInspector
{
    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    void OnSceneGUI(SceneView sceneView)
    {
        EditorUtility.SetDirty(target);
        MeshtaskSettings t = target as GrandstandSettings;
        Undo.RecordObject(t, "GrandstandSettings");
        Draw3DShape(t);
        DrawHandles(t);
        t.EDITOR_offSetPosition = DrawOffsetHandle(t);
    }
}
