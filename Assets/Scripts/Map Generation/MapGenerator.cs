using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    [Range(0, MeshGenerator.NUM_SUPPORTED_LOD - 1)]
    private int m_editorPreviewLOD = 0;

    [SerializeField]
    [Range(0, MeshGenerator.NUM_SUPPORTED_CHUNK_SIZES - 1)]
    private int m_chunkSizeIndex = 0;

    [SerializeField]
    private MapDisplay m_display;

    [SerializeField]
    private NoiseData m_noiseData = null;

    [SerializeField]
    private TerrainData m_terrainData = null;

    public bool autoUpdate;

    private float[,] m_falloffMap = null;

    private BackgroundTerrainChunkService m_backgroundTerrainChunkService = null;

    public TerrainData TerrainData
    {
        get { return m_terrainData; }
    }

    public BackgroundTerrainChunkService BackgroundTerrainChunkService { get { return m_backgroundTerrainChunkService; } }

    private void Awake()
    {
        m_falloffMap = FalloffMapGenerator.GenerateFalloffMap(MapChunkSize() + 2, MapChunkSize() + 2);
        m_backgroundTerrainChunkService = new BackgroundTerrainChunkService(MapChunkSize() + 2, m_noiseData, m_terrainData);
    }

    private void Start()
    {
        if (m_display == null)
            m_display = GetComponent<MapDisplay>();
    }

    private void Update()
    {
        m_backgroundTerrainChunkService.ProcessQueue();
    }

    public int MapChunkSize()
    {
        return MeshGenerator.SupportedChunkSizes[m_chunkSizeIndex] - 1;
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChunkSize() + 2, MapChunkSize() + 2, m_noiseData);

        ApplyFalloffMap(MapChunkSize() + 2, noiseMap);

        m_display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, m_terrainData.MeshHeightMultiplier, m_terrainData.MeshHeightCurve, m_editorPreviewLOD), m_terrainData.UniformScale);
    }

    private void ApplyFalloffMap(int size, float[,] mapData)
    {
        if (m_falloffMap == null)
            m_falloffMap = FalloffMapGenerator.GenerateFalloffMap(size, size);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                mapData[x, y] = Mathf.Clamp01(mapData[x, y] - m_falloffMap[x, y]);
            }
        }
    }

}