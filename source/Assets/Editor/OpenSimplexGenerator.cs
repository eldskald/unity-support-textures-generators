using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class OpenSimplexGenerator : EditorWindow {

    [MenuItem("Tools/Support Textures Generators/Open Simplex Generator")]
    public static void OpenWindow () => GetWindow<OpenSimplexGenerator>();

    private int _seed = 0;
    private Vector2Int _resolution = new Vector2Int(256, 256);
    private float _frequency = 1f;
    private int _octaves = 4;
    private float _persistance = 0.5f;
    private float _lacunarity = 2f;
    private float _power = 1.0f;
    private bool _inverted = false;

    private float[] _seeds;
    private Texture2D _preview;

    private void OnGUI () {

        // Seeds. We need a different seed for each octave, which is why we
        // use a seeds array.
        EditorGUI.BeginChangeCheck();
        _seed = EditorGUILayout.IntField("Seed", _seed);
        if (EditorGUI.EndChangeCheck()) {
            SetSeeds(_seed);
        }

    }

    private void SetSeeds (int newSeed) {
        System.Random rng = new System.Random(newSeed);
        for (int i = 0; i < 9; i++) {
            _seeds[i] = rng.Next(-100000, 100000);
        }
    }

    private float[,] GenerateValues (Vector2Int size) {
        float[,] values = new float[size.x, size.y];

        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                float sample = 0f;
                float amp = 1f;
                float freq = 1f;

                for (int k = 0; k < _octaves; k++) {
                    float iAngle = i * 2f * Mathf.PI / size.x;
                    float jAngle = j * 2f * Mathf.PI / size.y;
                    double nx = _frequency * Mathf.Sin(iAngle) / freq;
                    double ny = _frequency * Mathf.Cos(iAngle) / freq;
                    double nz = _frequency * Mathf.Sin(jAngle) / freq;
                    double nw = _frequency * Mathf.Cos(jAngle) / freq;
                    float noise = OpenSimplex2S.Noise4_UnskewedBase(
                        seeds[k], nx, ny, nz, nw);
                }
            }
        }
    }

}
