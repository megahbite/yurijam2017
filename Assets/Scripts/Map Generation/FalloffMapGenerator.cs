using UnityEngine;
using System.Collections;

public static class FalloffMapGenerator
{ 
    public static float[,] GenerateFalloffMap(int width, int height)
    {
        float[,] map = new float[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float x = i / (float)width * 2 - 1;
                float y = j / (float)height * 2 - 1;

                float value = 1 - Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    const float A = 3f;
    const float B = 0.5f;

    private static float Evaluate(float value)
    {
        return Mathf.Pow(value, A) / (Mathf.Pow(value, A) + Mathf.Pow(B - B * value, A));
    }
}
