using UnityEngine;

public static class Noise
{
    public enum NormalizeMode { Local, Global }; 
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseData noiseData)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        float maxPossibleHeight = 0;
        float amplitude = 1, frequency = 1;

        System.Random prng = new System.Random(noiseData.Seed);
        Vector2[] octaveOffsets = new Vector2[noiseData.Octaves];
        for (int i = 0; i < noiseData.Octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + noiseData.Offset.x;
            float offsetY = prng.Next(-100000, 100000) - noiseData.Offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
            maxPossibleHeight += amplitude;
            amplitude *= noiseData.Persistance;
        }

        float scale = noiseData.NoiseScale;
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < noiseData.Octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= noiseData.Persistance;
                    frequency *= noiseData.Lacunarity;
                }

                if (noiseData.NormalizeMode == NormalizeMode.Local)
                {
                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (noiseData.NormalizeMode == NormalizeMode.Local)
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 1.75f);
                    noiseMap[x, y] = normalizedHeight;
                }
            }
        }

        return noiseMap;
    }

}