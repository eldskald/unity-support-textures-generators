using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class GradientGenerator : EditorWindow {

    [MenuItem("Tools/Support Textures Generators/Gradient Texture Generator")]
    public static void OpenWindow () => GetWindow<GradientGenerator>();

    [SerializeField] Gradient _gradient;
    [SerializeField] AnimationCurve _curve;
    [SerializeField] Vector2Int _resolution;
    [SerializeField] string _path;

    SerializedObject _so;
    SerializedProperty _propResolution;
    SerializedProperty _propPath;

     private void OnEnable () {
        _so = new SerializedObject(this);
        _propResolution = _so.FindProperty("_resolution");
        _propPath = _so.FindProperty("_path");

        _gradient = new Gradient();
        _curve = new AnimationCurve();

        _resolution.x = EditorPrefs.GetInt(
            "TOOL_GRADIENTGENERATOR_resolution_x", 128);
        _resolution.y = EditorPrefs.GetInt(
            "TOOL_GRADIENTGENERATOR_resolution_y", 1);
        _path = EditorPrefs.GetString(
            "TOOL_GRADIENTGENERATOR_path", "Textures/new-gradient.png");
        
        this.minSize = new Vector2(300, 275);
     }

     private void OnDisable () {
        EditorPrefs.SetInt(
            "TOOL_GRADIENTGENERATOR_resolution_x", _resolution.x);
        EditorPrefs.SetInt(
            "TOOL_GRADIENTGENERATOR_resolution_y", _resolution.y);
        EditorPrefs.SetString(
            "TOOL_GRADIENTGENERATOR_path", _path);
     }

     private void OnGUI () {
         _so.Update();

        GUILayout.Space(16);
        _gradient = EditorGUILayout.GradientField("Gradient", _gradient);
        GUILayout.Space(8);
        _curve = EditorGUILayout.CurveField("Curve", _curve);

        GUILayout.Space(32);
        GUILayout.Label("Target File Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _propResolution.vector2IntValue = EditorGUILayout.Vector2IntField(
            "Texture Resolution", _propResolution.vector2IntValue);
        _propPath.stringValue = EditorGUILayout.TextField(
            "File Path", _propPath.stringValue);
        if (EditorGUI.EndChangeCheck()) {
            _so.ApplyModifiedProperties();
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
