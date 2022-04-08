using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class OpenSimplexGenerator : EditorWindow {

    [MenuItem("Tools/Support Textures Generators/Open Simplex Noise Generator")]
    public static void OpenWindow () => GetWindow<OpenSimplexGenerator>();

    [SerializeField] int _seed;
    [SerializeField] float _frequency;
    [SerializeField] int _octaves;
    [SerializeField] float _persistance;
    [SerializeField] float _lacunarity;
    [SerializeField] float _rangeMin;
    [SerializeField] float _rangeMax;
    [SerializeField] float _power;
    [SerializeField] bool _inverted;
    [SerializeField] Vector2Int _resolution;
    [SerializeField] string _path;

    private SerializedObject _so;
    private SerializedProperty _propSeed;
    private SerializedProperty _propFrequency;
    private SerializedProperty _propOctaves;
    private SerializedProperty _propPersistance;
    private SerializedProperty _propLacunarity;
    private SerializedProperty _propRangeMin;
    private SerializedProperty _propRangeMax;
    private SerializedProperty _propPower;
    private SerializedProperty _propInverted;
    private SerializedProperty _propResolution;
    private SerializedProperty _propPath;

    private long[] _seeds;
    private Texture2D _preview;

    private void OnEnable () {
        _so = new SerializedObject(this);
        _propSeed = _so.FindProperty("_seed");
        _propFrequency = _so.FindProperty("_frequency");
        _propOctaves = _so.FindProperty("_octaves");
        _propPersistance = _so.FindProperty("_persistance");
        _propLacunarity = _so.FindProperty("_lacunarity");
        _propRangeMin = _so.FindProperty("_rangeMin");
        _propRangeMax = _so.FindProperty("_rangeMax");
        _propPower = _so.FindProperty("_power");
        _propInverted = _so.FindProperty("_inverted");
        _propResolution = _so.FindProperty("_resolution");
        _propPath = _so.FindProperty("_path");

        _seed = EditorPrefs.GetInt(
            "TOOL_OPENSIMPLEXGENERATOR_seed", 0);
        _frequency = EditorPrefs.GetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_frequency", 1f);
        _octaves = EditorPrefs.GetInt(
            "TOOL_OPENSIMPLEXGENERATOR_octaves", 1);
        _persistance = EditorPrefs.GetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_persistance", 0.5f);
        _lacunarity = EditorPrefs.GetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_lacunarity", 1f);
        _rangeMin = EditorPrefs.GetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_rangeMin", 0f);
        _rangeMax = EditorPrefs.GetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_rangeMax", 1f);
        _power = EditorPrefs.GetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_power", 1f);
        _inverted = EditorPrefs.GetBool(
            "TOOL_OPENSIMPLEXGENERATOR_inverted", false);
        _resolution.x = EditorPrefs.GetInt(
            "TOOL_OPENSIMPLEXGENERATOR_resolution_x", 256);
        _resolution.y = EditorPrefs.GetInt(
            "TOOL_OPENSIMPLEXGENERATOR_resolution_y", 256);
        _path = EditorPrefs.GetString(
            "TOOL_OPENSIMPLEXGENERATOR_path", "Textures/new-noise.png");

        SetSeeds(_seed);
        _preview = GenerateTexture(192, 192);

        this.minSize = new Vector2(300, 650);
    }

    private void OnDisable() {
        EditorPrefs.SetInt(
            "TOOL_OPENSIMPLEXGENERATOR_seed", _seed);
        EditorPrefs.SetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_frequency", _frequency);
        EditorPrefs.SetInt(
            "TOOL_OPENSIMPLEXGENERATOR_octaves", _octaves);
        EditorPrefs.SetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_persistance", _persistance);
        EditorPrefs.SetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_lacunarity", _lacunarity);
        EditorPrefs.SetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_rangeMin", _rangeMin);
        EditorPrefs.SetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_rangeMax", _rangeMax);
        EditorPrefs.SetFloat(
            "TOOL_OPENSIMPLEXGENERATOR_power", _power);
        EditorPrefs.SetBool(
            "TOOL_OPENSIMPLEXGENERATOR_inverted", _inverted);
        EditorPrefs.SetInt(
            "TOOL_OPENSIMPLEXGENERATOR_resolution_x", _resolution.x);
        EditorPrefs.SetInt(
            "TOOL_OPENSIMPLEXGENERATOR_resolution_y", _resolution.y);
        EditorPrefs.SetString(
            "TOOL_OPENSIMPLEXGENERATOR_path", _path);
    }

    private void OnGUI () {
        _so.Update();

        // Seeds. We need a different seed for each octave, which is why we
        // use a seeds array.
        EditorGUI.BeginChangeCheck();
        _propSeed.intValue = EditorGUILayout.IntField(
            "Seed", _propSeed.intValue);
        if (EditorGUI.EndChangeCheck()) {
            SetSeeds(_propSeed.intValue);
            _so.ApplyModifiedProperties();
            _preview = GenerateTexture(_preview.width, _preview.height);
        }

        // Noise settings.
        EditorGUI.BeginChangeCheck();
        _propFrequency.floatValue = EditorGUILayout.FloatField(
            "Frequency", _propFrequency.floatValue);
        
        GUILayout.Space(16);
        GUILayout.Label("Fractal Settings", EditorStyles.boldLabel);
        _propOctaves.intValue = EditorGUILayout.IntSlider(
            "Octaves", _propOctaves.intValue, 1, 9);
        _propPersistance.floatValue = EditorGUILayout.Slider(
            "Persistance", _propPersistance.floatValue, 0f, 1f);
        _propLacunarity.floatValue = EditorGUILayout.Slider(
            "Lacunarity", _propLacunarity.floatValue, 0.1f, 4.0f);
        
        GUILayout.Space(16);
        GUILayout.Label("Modifiers", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Range:", _rangeMin.ToString() + " to " + _rangeMax.ToString());
        EditorGUILayout.MinMaxSlider(ref _rangeMin, ref _rangeMax, 0f, 1f);
        _propRangeMin.floatValue = _rangeMin;
        _propRangeMax.floatValue = _rangeMax;
        _propPower.floatValue = EditorGUILayout.Slider(
            "Interpolation Power", _propPower.floatValue, 1f, 8f);
        _propInverted.boolValue = EditorGUILayout.Toggle(
            "Inverted", _propInverted.boolValue);
        if (EditorGUI.EndChangeCheck()) {
            _so.ApplyModifiedProperties();
            _frequency = _frequency < 0f ? 0f : _frequency;
            _preview = GenerateTexture(_preview.width, _preview.height);
        }

        // Texture settings. They don't cause the preview to change.
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

        // Draw preview texture.
        GUILayout.Space(10);
        EditorGUI.DrawPreviewTexture(new Rect(32, 380, 192, 192), _preview);

        // Save button.
        GUILayout.Space(236);
        if (GUILayout.Button("Save Texture")) {
            Texture2D tex = GenerateTexture(_resolution.x, _resolution.y);
            byte[] data = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(
                string.Format("{0}/{1}", Application.dataPath, _path), data);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
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
                float frequency = _frequency;

                for (int k = 0; k < _octaves; k++) {
                    float iAngle = i * 2f * Mathf.PI / width;
                    float jAngle = j * 2f * Mathf.PI / height;
                    double nx = frequency * Mathf.Sin(iAngle);
                    double ny = frequency * Mathf.Cos(iAngle);
                    double nz = frequency * Mathf.Sin(jAngle);
                    double nw = frequency * Mathf.Cos(jAngle);
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
                value = Mathf.InverseLerp(_rangeMin, _rangeMax, value);
                
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
