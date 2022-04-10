using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class GradientGenerator : EditorWindow {

    [MenuItem("Tools/Support Textures Generators/Gradient Texture Generator")]
    public static void OpenWindow () => GetWindow<GradientGenerator>();

    private Gradient _gradient;
    private AnimationCurve _curve;
    private Vector2Int _resolution;
    private string _path;

     private void OnEnable () {
        _gradient = new Gradient();
        _curve = new AnimationCurve();

        // EditorPrefs to load settings when you last used it.
        _resolution.x = EditorPrefs.GetInt(
            "TOOL_GRADIENTGENERATOR_resolution_x", 128);
        _resolution.y = EditorPrefs.GetInt(
            "TOOL_GRADIENTGENERATOR_resolution_y", 1);
        _path = EditorPrefs.GetString(
            "TOOL_GRADIENTGENERATOR_path", "Textures/new-gradient.png");
        
        this.minSize = new Vector2(300, 275);
     }

     private void OnDisable () {

        // EditorPrefs to save settings for when you next use it.
        EditorPrefs.SetInt(
            "TOOL_GRADIENTGENERATOR_resolution_x", _resolution.x);
        EditorPrefs.SetInt(
            "TOOL_GRADIENTGENERATOR_resolution_y", _resolution.y);
        EditorPrefs.SetString(
            "TOOL_GRADIENTGENERATOR_path", _path);
     }

     private void OnGUI () {
        GUILayout.Space(16);
        _gradient = EditorGUILayout.GradientField(
            "Gradient", _gradient);
        GUILayout.Space(8);
        _curve = EditorGUILayout.CurveField(
            "Curve", _curve);

        GUILayout.Space(32);
        GUILayout.Label("Target File Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _resolution = EditorGUILayout.Vector2IntField(
            "Texture Resolution", _resolution);
        _path = EditorGUILayout.TextField(
            "File Path", _path);
        if (EditorGUI.EndChangeCheck()) {
            _resolution.x = _resolution.x < 1 ? 1 : _resolution.x;
            _resolution.y = _resolution.y < 1 ? 1 : _resolution.y;
        }

        GUILayout.Space(32);
        if (GUILayout.Button("Save from Gradient")) {
            Texture2D tex = new Texture2D(_resolution.x, _resolution.y);
            for (int i = 0; i < _resolution.x; i++) {
                Color value = _gradient.Evaluate(i / (float)_resolution.x);
                for (int j = 0; j < _resolution.y; j++) {
                    tex.SetPixel(i, j, value);
                }
            }
            byte[] data = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(
                string.Format("{0}/{1}", Application.dataPath, _path), data);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        GUILayout.Space(8);
        if (GUILayout.Button("Save from Curve")) {
            Texture2D tex = new Texture2D(_resolution.x, _resolution.y);
            for (int i = 0; i < _resolution.x; i++) {
                float value = _curve.Evaluate(i / (float)_resolution.x);
                value = Mathf.Clamp01(value);
                for (int j = 0; j < _resolution.y; j++) {
                    tex.SetPixel(i, j, new Color(value, value, value, 1f));
                }
            }
            byte[] data = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(
                string.Format("{0}/{1}", Application.dataPath, _path), data);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
     }
}
