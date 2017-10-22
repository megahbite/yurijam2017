using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BackgroundTerrainChunkService : IBackgroundChunkService
{
    private int m_mapSize = 0;
    private NoiseData m_noiseData = null;
    private TerrainData m_terrainData = null;

    private Queue<MapThreadInfo<float[,]>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<float[,]>>();
    private Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public BackgroundTerrainChunkService(int mapSize, NoiseData terrainNoiseData, TerrainData terrainData)
    {
        m_mapSize = mapSize;
        m_noiseData = terrainNoiseData;
        m_terrainData = terrainData;
    }

    public void RequestMapData(Vector2 centre, Action<float[,]> callback)
    {
        ThreadStart threadStart = () =>
        {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    public void RequestMeshData(float[,] heightMap, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = () => { MeshDataThread(heightMap, lod, callback); };

        new Thread(threadStart).Start();
    }

    private void MapDataThread(Vector2 centre, Action<float[,]> callback)
    {
        int size = m_mapSize;
        float[,] mapData = Noise.GenerateNoiseMap(size, size, m_noiseData);

        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<float[,]>(callback, mapData));
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

    public void ProcessQueue()
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

    
}
