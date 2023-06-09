using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ProGen))]
public class ProGenEditor : Editor
{
    private ProGen proGen;

    private Editor themeEditor;

    private void OnEnable() 
    {
        proGen = (ProGen)target;
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Generate"))
        {
            proGen.Generate();
        }

        DrawSettingsOnEditor(proGen.Theme, proGen.Generate, true, ref themeEditor);
    }

    void DrawSettingsOnEditor(Object settings, System.Action onSettingsUpdated, bool foldout, ref Editor editor)
    {
        if(settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);

            using(var check = new EditorGUI.ChangeCheckScope())
            {
                CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();

                if(check.changed)
                {
                    if(onSettingsUpdated != null)
                    {
                        onSettingsUpdated();
                    }
                }
            }
        }
    }
}
