using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : ScriptableObject
{
    public float UniformScale;

    public float MeshHeightMultiplier;

    public AnimationCurve MeshHeightCurve;
}
