using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class OpenSimplexGenerator : EditorWindow {

    [MenuItem("Tools/Support Textures Generators/Open Simplex Generator")]
    public static void OpenWindow () => GetWindow<OpenSimplexGenerator>();

    [SerializeField] private int _seed = 0;
    [SerializeField] private float _frequency = 1f;
    [SerializeField] private int _octaves = 4;
    [SerializeField] private float _persistance = 0.5f;
    [SerializeField] private float _lacunarity = 2f;
    [SerializeField] private float _power = 1.0f;
    [SerializeField] private bool _inverted = false;
    [SerializeField] private Vector2Int _resolution = new Vector2Int(256, 256);
    [SerializeField] private string _path = "Textures/new-noise.png";

    private long[] _seeds;
    private Texture2D _preview;

    private void OnEnable () {
        SetSeeds(_seed);
        _preview = GenerateTexture(192, 192);
    }

    private void OnGUI () {
        
        // Seeds. We need a different seed for each octave, which is why we
        // use a seeds array.
        EditorGUI.BeginChangeCheck();
        _seed = EditorGUILayout.IntField("Seed", _seed);
        if (EditorGUI.EndChangeCheck()) {
            SetSeeds(_seed);
            _preview = GenerateTexture(_preview.width, _preview.height);
        }

        // Noise settings.
        GUILayout.Space(10);
        GUILayout.Label("Noise Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _frequency = EditorGUILayout.FloatField("Frequency", _frequency);
        _octaves = EditorGUILayout.IntSlider("Octaves", _octaves, 1, 9);
        _persistance = EditorGUILayout.Slider(
            "Persistance", _persistance, 0f, 1f);
        _lacunarity = EditorGUILayout.Slider(
            "Lacunarity", _lacunarity, 0.1f, 4.0f);
        _power = EditorGUILayout.Slider("Power", _power, 1f, 8f);
        _inverted = EditorGUILayout.Toggle("Inverted", _inverted);
        if (EditorGUI.EndChangeCheck()) {
            _frequency = _frequency < 0f ? 0f : _frequency;
            _preview = GenerateTexture(_preview.width, _preview.height);
        }

        // Texture settings. They don't cause the preview to change.
        GUILayout.Space(10);
        GUILayout.Label("Target File Settings", EditorStyles.boldLabel);
        _resolution = EditorGUILayout.Vector2IntField(
            "Texture Resolution", _resolution);
        _path = EditorGUILayout.TextField("File Path", _path);

        // Draw preview texture.
        GUILayout.Space(10);
        EditorGUI.DrawPreviewTexture(new Rect(16, 300, 192, 192), _preview);

        // Save button.
        GUILayout.Space(256);
        if (GUILayout.Button("Save Texture")) {
            Texture2D tex = GenerateTexture(_resolution.x, _resolution.y);
            byte[] data = tex.EncodeToPNG();
            File.WriteAllBytes(
                string.Format("{0}/{1}", Application.dataPath, _path), data);
        }
    }

    private void SetSeeds (int newSeed) {
        System.Random rng = new System.Random(newSeed);
        _seeds = new long[9];
        for (int i = 0; i < 9; i++) {
            _seeds[i] = rng.Next(-100000, 100000);
        }
    }

    private Texture2D GenerateTexture (int width, int height) {
        float[,] values = new float[width, height];
        Texture2D tex = new Texture2D(width, height);

        // This next block is for filling the array. We will later turn this
        // array into a texture. We're doing the torus mapping on 4D.
        float maxValue = Mathf.NegativeInfinity;  // We set these so we can
        float minValue = Mathf.Infinity;          // normalize values later.
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                float sample = 0f;
                float amplitude = 1f;
                float frequency = 1f;

                for (int k = 0; k < _octaves; k++) {
                    float iAngle = i * 2f * Mathf.PI / width;
                    float jAngle = j * 2f * Mathf.PI / height;
                    double nx = _frequency * Mathf.Sin(iAngle) / frequency;
                    double ny = _frequency * Mathf.Cos(iAngle) / frequency;
                    double nz = _frequency * Mathf.Sin(jAngle) / frequency;
                    double nw = _frequency * Mathf.Cos(jAngle) / frequency;
                    float noise = OpenSimplex2S.Noise4_Fallback(
                        _seeds[k], nx, ny, nz, nw);
                    sample += noise * amplitude;
                    amplitude *= _persistance;
                    frequency *= _lacunarity;
                }

                values[i,j] = sample;
                maxValue = sample > maxValue ? sample : maxValue;
                minValue = sample < minValue ? sample : minValue;
            }
        }

        // For the final touches, we normalize, apply power and inverse.
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                float value = values[i, j];
                value = Mathf.InverseLerp(minValue, maxValue, value);
                
                float k = Mathf.Pow(2f, _power - 1f);
                if (value < 0.5f) {
                    value = k * Mathf.Pow(value, _power);
                }
                else {
                    value = 1f - k * Mathf.Pow(1f - value, _power);
                }

                value = _inverted ? 1f - value : value;

                // Finally, we make the texture.
                tex.SetPixel(i, j, new Color(value, value, value, 1.0f));
            }
        }
        tex.Apply();
        return tex;
    }

}
