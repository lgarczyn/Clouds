using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (AltitudeMap))]
public class AltitudeMapEditor : Editor {

    AltitudeMap altitude;
    Editor altitudeSettingsEditor;

    public override void OnInspectorGUI () {
        DrawDefaultInspector ();

        if (altitude.altitudeSettings != null) {
            DrawSettingsEditor (altitude.altitudeSettings, ref altitude.showSettingsEditor, ref altitudeSettingsEditor);
            altitude.UpdateMap();
        }

    }



    void DrawSettingsEditor (Object settings, ref bool foldout, ref Editor editor) {
        if (settings != null) {
            foldout = EditorGUILayout.InspectorTitlebar (foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope ()) {
                if (foldout) {
                    CreateCachedEditor (settings, null, ref editor);
                    editor.OnInspectorGUI ();
                }
            }
        }
    }

    void OnEnable () {
        altitude = (AltitudeMap) target;
    }

}