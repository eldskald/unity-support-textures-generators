using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class VoronoiGenerator : EditorWindow {

    [MenuItem("Tools/Support Textures Generators/Voronoi Noise Generator")]
    public static void OpenWindow () => GetWindow<VoronoiGenerator>();

    private enum CombinationMode {
        First = 0,
        SecondMinusFirst = 1
    }

    [SerializeField] float _variation;
    [SerializeField] CombinationMode _combination;
    [SerializeField] float _frequency;
    [SerializeField] int _octaves;
    [SerializeField] float _persistance;
    [SerializeField] float _lacunarity;
    [SerializeField] float _jitter;
    [SerializeField] float _scale;
    [SerializeField] float _offset;
    [SerializeField] float _rangeMin;
    [SerializeField] float _rangeMax;
    [SerializeField] float _power;
    [SerializeField] bool _inverted;
    [SerializeField] Vector2Int _resolution;
    [SerializeField] string _path;
    
    SerializedObject so;
    SerializedProperty propVariation;
    SerializedProperty propCombination;
    SerializedProperty propFrequency;
    SerializedProperty propOctaves;
    SerializedProperty propPersistance;
    SerializedProperty propLacunarity;
    SerializedProperty propJitter;
    SerializedProperty propScale;
    SerializedProperty propOffset;
    SerializedProperty propRangeMin;
    SerializedProperty propRangeMax;
    SerializedProperty propPower;
    SerializedProperty propInverted;
    SerializedProperty propResolution;
    SerializedProperty propPath;

    private Material _material;
    private Texture2D _preview;

    private void OnEnable () {
        so = new SerializedObject(this);
        propVariation = so.FindProperty("_variation");
        propCombination = so.FindProperty("_combination");
        propFrequency = so.FindProperty("_frequency");
        propOctaves = so.FindProperty("_octaves");
        propPersistance = so.FindProperty("_persistance");
        propLacunarity = so.FindProperty("_lacunarity");
        propJitter = so.FindProperty("_jitter");
        propScale = so.FindProperty("_scale");
        propOffset = so.FindProperty("_offset");
        propRangeMin = so.FindProperty("_rangeMin");
        propRangeMax = so.FindProperty("_rangeMax");
        propPower = so.FindProperty("_power");
        propInverted = so.FindProperty("_inverted");
        propResolution = so.FindProperty("_resolution");
        propPath = so.FindProperty("_path");

        _variation = EditorPrefs.GetFloat(
            "TOOL_VORONOIGENERATOR_variation", 0f);
        _combination = (CombinationMode)EditorPrefs.GetInt(
            "TOOL_VORONOIGENERATOR_combination", 0);
        _frequency = EditorPrefs.GetFloat(
            "TOOL_VORONOIGENERATOR_frequency", 1f);
        _octaves = EditorPrefs.GetInt(
            "TOOL_VORONOIGENERATOR_octaves", 1);
        _persistance = EditorPrefs.GetFloat(
            "TOOL_VORONOIGENERATOR_persistance", 0.5f);
        _lacunarity = EditorPrefs.GetFloat(
            "TOOL_VORONOIGENERATOR_lacunarity", 2f);
        _jitter = EditorPrefs.GetFloat(
            "TOOL_VORONOIGENERATOR_jitter", 1f);
        _scale = EditorPrefs.GetFloat(
            "TOOL_VORONOIGENERATOR_scale", 1f);
        _offset = EditorPrefs.GetFloat(
            "TOOL_VORONOIGENERATOR_offset", 0f);
        _rangeMin = EditorPrefs.GetFloat(
            "TOOL_VORONOIGENERATOR_rangeMin", 0f);
        _rangeMax = EditorPrefs.GetFloat(
            "TOOL_VORONOIGENERATOR_rangeMax", 1f);
        _power = EditorPrefs.GetFloat(
            "TOOL_VORONOIGENERATOR_power", 1f);
        _inverted = EditorPrefs.GetBool(
            "TOOL_VORONOIGENERATOR_inverted", false);
        _resolution.x = EditorPrefs.GetInt(
            "TOOL_VORONOIGENERATOR_resolution_x", 256);
        _resolution.y = EditorPrefs.GetInt(
            "TOOL_VORONOIGENERATOR_resolution_y", 256);
        _path = EditorPrefs.GetString(
            "TOOL_VORONOIGENERATOR_path", "Textures/new-noise.png");
        
        _material = new Material(Shader.Find("Editor/VoronoiNoiseGenerator"));
        UpdateMaterial();
        _preview = GeneratePreview(192, 192);
    }

    private void OnDisable () {
        EditorPrefs.SetFloat(
            "TOOL_VORONOIGENERATOR_variation", _variation);
        EditorPrefs.SetInt(
            "TOOL_VORONOIGENERATOR_combination", (int)_combination);
        EditorPrefs.SetFloat(
            "TOOL_VORONOIGENERATOR_frequency", _frequency);
        EditorPrefs.SetInt(
            "TOOL_VORONOIGENERATOR_octaves", _octaves);
        EditorPrefs.SetFloat(
            "TOOL_VORONOIGENERATOR_persistance", _persistance);
        EditorPrefs.SetFloat(
            "TOOL_VORONOIGENERATOR_lacunarity", _lacunarity);
        EditorPrefs.SetFloat(
            "TOOL_VORONOIGENERATOR_jitter", _jitter);
        EditorPrefs.SetFloat(
            "TOOL_VORONOIGENERATOR_scale", _scale);
        EditorPrefs.SetFloat(
            "TOOL_VORONOIGENERATOR_offset", _offset);
        EditorPrefs.SetFloat(
            "TOOL_VORONOIGENERATOR_rangeMin", _rangeMin);
        EditorPrefs.SetFloat(
            "TOOL_VORONOIGENERATOR_rangeMax", _rangeMax);
        EditorPrefs.SetFloat(
            "TOOL_VORONOIGENERATOR_power", _power);
        EditorPrefs.SetBool(
            "TOOL_VORONOIGENERATOR_inverted", _inverted);
        EditorPrefs.SetInt(
            "TOOL_VORONOIGENERATOR_resolution_x", _resolution.x);
        EditorPrefs.SetInt(
            "TOOL_VORONOIGENERATOR_resolution_y", _resolution.y);
        EditorPrefs.SetString(
            "TOOL_VORONOIGENERATOR_path", _path);
    }

    private void OnGUI () {
        so.Update();

        // Noise settings.
        GUILayout.Label("Noise Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        propVariation.floatValue = EditorGUILayout.FloatField(
            "Variation", propVariation.floatValue);
        propCombination.intValue = (int)(CombinationMode)(
            EditorGUILayout.EnumPopup(
            "Combination Mode", (CombinationMode)propCombination.intValue));
        propFrequency.floatValue = EditorGUILayout.FloatField(
            "Frequency", propFrequency.floatValue);
        propOctaves.intValue = EditorGUILayout.IntSlider(
            "Octaves", propOctaves.intValue, 1, 9);
        propPersistance.floatValue = EditorGUILayout.Slider(
            "Persistance", propPersistance.floatValue, 0f, 1f);
        propLacunarity.floatValue = EditorGUILayout.Slider(
            "Lacunarity", propLacunarity.floatValue, 0.1f, 4.0f);
        propScale.floatValue = EditorGUILayout.FloatField(
            "Scale", propScale.floatValue);
        propOffset.floatValue = EditorGUILayout.FloatField(
            "Offset", propOffset.floatValue);
        EditorGUILayout.LabelField(
            "Range:", _rangeMin.ToString() + " to " + _rangeMax.ToString());
        EditorGUILayout.MinMaxSlider(ref _rangeMin, ref _rangeMax, 0f, 1f);
        propRangeMin.floatValue = _rangeMin;
        propRangeMax.floatValue = _rangeMax;
        propPower.floatValue = EditorGUILayout.Slider(
            "Power", propPower.floatValue, 1f, 8f);
        propInverted.boolValue = EditorGUILayout.Toggle(
            "Inverted", propInverted.boolValue);
        if (EditorGUI.EndChangeCheck()) {
            so.ApplyModifiedProperties();
            _frequency = _frequency < 0f ? 0f : _frequency;
            _scale = _scale < 0f ? 0f : _scale;
            UpdateMaterial();
            _preview = GeneratePreview(_preview.width, _preview.height);
        }

        // Texture settings. They don't cause the preview to change.
        GUILayout.Space(10);
        GUILayout.Label("Target File Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        propResolution.vector2IntValue = EditorGUILayout.Vector2IntField(
            "Texture Resolution", propResolution.vector2IntValue);
        propPath.stringValue = EditorGUILayout.TextField(
            "File Path", propPath.stringValue);
        if (EditorGUI.EndChangeCheck()) {
            so.ApplyModifiedProperties();
            _resolution.x = _resolution.x < 1 ? 1 : _resolution.x;
            _resolution.y = _resolution.y < 1 ? 1 : _resolution.y;
        }

        // Draw preview texture.
        GUILayout.Space(10);
        EditorGUI.DrawPreviewTexture(new Rect(32, 370, 192, 192), _preview);

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

    private void UpdateMaterial () {
        _material.SetFloat("_Variation", _variation);
        switch (_combination) {
            case CombinationMode.First:
                _material.EnableKeyword("_COMBINATION_ONE");
                _material.DisableKeyword("_COMBINATION_TWO");
                break;
            case CombinationMode.SecondMinusFirst:
                _material.EnableKeyword("_COMBINATION_TWO");
                _material.DisableKeyword("_COMBINATION_ONE");
                break;
        }
        _material.SetFloat("_Frequency", _frequency);
        _material.SetFloat("_Octaves", (float)_octaves);
        _material.SetFloat("_Persistance", _persistance);
        _material.SetFloat("_Lacunarity", _lacunarity);
        _material.SetFloat("_Jitter", _jitter);
        _material.SetFloat("_Scale", _scale);
        _material.SetFloat("_Offset", _offset);
        _material.SetFloat("_RangeMin", _rangeMin);
        _material.SetFloat("_RangeMax", _rangeMax);
        _material.SetFloat("_Power", _power);
        switch (_inverted) {
            case true:
                _material.EnableKeyword("_INVERTED");
                break;
            case false:
                _material.DisableKeyword("_INVERTED");
                break;
        }
    }

    private Texture2D GeneratePreview (int width, int height) {
        Texture2D tex = new Texture2D(
            width, height, TextureFormat.ARGB32, false);
        RenderTexture temp = RenderTexture.GetTemporary(
            width, height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(tex, temp, _material);
        Graphics.CopyTexture(temp, tex);
        RenderTexture.ReleaseTemporary(temp);
        return tex;
    }

    private Texture2D GenerateTexture (int width, int height) {
        Texture2D tex = new Texture2D(
            width, height, TextureFormat.ARGB32, false);
        RenderTexture temp = RenderTexture.GetTemporary(
            width, height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(tex, temp, _material);

        // We can't just .CopyTexture() and .EncodeToPNG() because
        // .EncodeToPNG() grabs what's on the texture on the RAM, but
        // .CopyTexture() only changes the texture on the GPU. In order
        // to bring the GPU memory back into the CPU, we need a .ReadPixels()
        // call, whence why the existence of this whole function.
        RenderTexture.active = temp;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(temp);

        return tex;
    }

}

