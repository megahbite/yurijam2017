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

    private Queue<MapThreadInfo<float[,]>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<float[,]>>();
    private Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private float[,] m_falloffMap = null;

    public TerrainData TerrainData
    {
        get { return m_terrainData; }
    }

    private void Awake()
    {
        m_falloffMap = FalloffMapGenerator.GenerateFalloffMap(MapChunkSize() + 2, MapChunkSize() + 2);
    }

    private void Start()
    {
        if (m_display == null)
            m_display = GetComponent<MapDisplay>();
    }

    public int MapChunkSize()
    {
        return MeshGenerator.SupportedChunkSizes[m_chunkSizeIndex] - 1;
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChunkSize() + 2, MapChunkSize() + 2, m_noiseData.Seed, m_noiseData.NoiseScale, 
            m_noiseData.Octaves, m_noiseData.Persistance, m_noiseData.Lacunarity, m_noiseData.Offset, m_noiseData.NormalizeMode);

        m_display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, m_terrainData.MeshHeightMultiplier, m_terrainData.MeshHeightCurve, m_editorPreviewLOD));
    }

    public void RequestMapData(Vector2 centre, bool useFalloffMap, Action<float[,]> callback)
    {
        ThreadStart threadStart = () =>
        {
            MapDataThread(centre, useFalloffMap, callback);
        };

        new Thread(threadStart).Start();
    }

    public void RequestMeshData(float[,] heightMap, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = () => { MeshDataThread(heightMap, lod, callback); };

        new Thread(threadStart).Start();
    }

    private void MapDataThread(Vector2 centre, bool useFalloffMap, Action<float[,]> callback)
    {
        int size = MapChunkSize() + 2;
        float[,] mapData = Noise.GenerateNoiseMap(size, size, m_noiseData.Seed, m_noiseData.NoiseScale,
            m_noiseData.Octaves, m_noiseData.Persistance, m_noiseData.Lacunarity, m_noiseData.Offset + centre, m_noiseData.NormalizeMode);

        if (useFalloffMap)
        {
            ApplyFalloffMap(size, mapData);
        }

        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<float[,]>(callback, mapData));
        }
    }

    private void ApplyFalloffMap(int size, float[,] mapData)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                mapData[x, y] = Mathf.Clamp01(mapData[x, y] - m_falloffMap[x, y]);
            }
        }
    }

    private void MeshDataThread(float[,] heightMap, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap, m_terrainData.MeshHeightMultiplier, m_terrainData.MeshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        while (mapDataThreadInfoQueue.Count > 0)
        {
            MapThreadInfo<float[,]> threadInfo = mapDataThreadInfoQueue.Dequeue();
            threadInfo.callback(threadInfo.parameter);
        }

        while (meshDataThreadInfoQueue.Count > 0)
        {
            MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
            threadInfo.callback(threadInfo.parameter);
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }
}