using UnityEngine;
using System.Collections;
using System;

public interface IBackgroundChunkService
{
    void RequestMapData(Vector2 centre, Action<float[,]> callback);
    void RequestMeshData(float[,] heightMap, int lod, Action<MeshData> callback);
    void ProcessQueue();
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