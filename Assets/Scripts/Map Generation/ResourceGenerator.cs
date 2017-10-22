using UnityEngine;
using System.Collections;

public class ResourceGenerator : MonoBehaviour
{
    [SerializeField]
    private ResourceData[] m_resources = null;

    [SerializeField]
    private Material m_terrainMaterial = null;

    [SerializeField]
    private MapGenerator m_mapGenerator = null;

    private void Awake()
    {
        int size = m_mapGenerator.MapChunkSize();
        foreach (ResourceData resource in m_resources)
        {
            Noise.GenerateNoiseMap(size + 2, size + 2, resource);
        }
    }
}
