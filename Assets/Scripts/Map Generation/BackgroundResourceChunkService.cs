using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BackgroundResourceChunkService : IBackgroundChunkService
{
    private int m_mapSize;
    private NoiseData m_noiseData;

    private Queue<MapThreadInfo<float[,]>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<float[,]>>();

    public BackgroundResourceChunkService(int mapSize, NoiseData resourceNoiseData)
    {
        m_mapSize = mapSize;
        m_noiseData = resourceNoiseData;
    }

    public void ProcessQueue()
    {
        while (mapDataThreadInfoQueue.Count > 0)
        {
            MapThreadInfo<float[,]> threadInfo = mapDataThreadInfoQueue.Dequeue();
            threadInfo.callback(threadInfo.parameter);
        }
    }

    public void RequestMapData(Vector2 centre, Action<float[,]> callback)
    {
        ThreadStart threadStart = () =>
        {
            MapDataThread(centre, callback);
        };

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

    public void RequestMeshData(float[,] heightMap, int lod, Action<MeshData> callback)
    {
        throw new NotImplementedException();
    }
}
