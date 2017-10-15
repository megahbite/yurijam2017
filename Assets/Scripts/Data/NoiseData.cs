using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : ScriptableObject
{
    public Noise.NormalizeMode NormalizeMode;

    public float NoiseScale;

    public int Octaves;

    [Range(0, 1)]
    public float Persistance;

    public float Lacunarity;

    public int Seed;

    public Vector2 Offset;

    void OnValidate()
    {
        if (Lacunarity < 1)
            Lacunarity = 1;
        if (Octaves < 0)
            Octaves = 0;
    }
}
