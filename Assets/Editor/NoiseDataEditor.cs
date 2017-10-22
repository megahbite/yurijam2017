using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseData))]
public class NoiseDataEditor : Editor
{
    NoiseData data;

    private void OnEnable()
    {
        data = (NoiseData)target;
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(Mathf.FloorToInt(r.width), Mathf.FloorToInt(r.height), data);

        GUI.DrawTexture(r, TextureGenerator.TextureFromHeightMap(noiseMap));
    }
}