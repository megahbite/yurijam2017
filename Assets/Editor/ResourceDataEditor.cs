using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ResourceData))]
public class ResourceDataEditor : Editor
{
    ResourceData data;

    private void OnEnable()
    {
        data = (ResourceData)target;
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        //if (data.NoiseProfile == null)
        //{
        //    GUI.DrawTexture(r, Texture2D.blackTexture);
        //    return;
        //}

        int width = Mathf.FloorToInt(r.width);
        int height = Mathf.FloorToInt(r.height);
        float[,] noiseMap = Noise.GenerateNoiseMap(width, height, data);
        float range = 1.0f - data.Threshold;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (noiseMap[x, y] < data.Threshold)
                    noiseMap[x, y] = 0f;
                else
                    noiseMap[x, y] = (noiseMap[x, y] - data.Threshold) / range;
            }
        }

        Color[] colourMap = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = BlendColors(data.Colour, Color.white, noiseMap[x, y]);
            }
        }

        GUI.DrawTexture(r, TextureGenerator.TextureFromColourMap(colourMap, width, height));
    }

    private static Color BlendColors(Color color, Color backColor, float amount)
    {
        float r = ((color.r * amount) + backColor.r * (1 - amount));
        float g = ((color.g * amount) + backColor.g * (1 - amount));
        float b = ((color.b * amount) + backColor.b * (1 - amount));
        return new Color(r, g, b);
    }
}