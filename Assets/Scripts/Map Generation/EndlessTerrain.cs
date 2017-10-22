using System;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    private static float s_maxViewDst;

    private const float VIEWER_CHUNK_UPDATE_DELTA = 25f;
    private const float SQR_VIEWER_CHUNK_UPDATE_DELTA = VIEWER_CHUNK_UPDATE_DELTA * VIEWER_CHUNK_UPDATE_DELTA;
    private const float COLLIDER_GEN_THRESHOLD = 5f;

    [SerializeField]
    private Transform m_viewer = null;

    [SerializeField]
    private Material m_mapMaterial = null;

    [SerializeField]
    private LODInfo[] m_levelsOfDetail = null;

    private int m_colliderLODIndex = 0;

    private static Vector2 s_viewerPosition;
    private Vector2 m_oldViewerPosition;
    static MapGenerator s_mapGenerator;
    int m_chunkSize;
    int m_chunksVisibleInViewDst;

    static Dictionary<Vector2, TerrainChunk> s_terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private static List<TerrainChunk> s_visibleTerrainChunks = new List<TerrainChunk>();

    void Start()
    {
        s_mapGenerator = GetComponent<MapGenerator>();
        s_maxViewDst = m_levelsOfDetail[m_levelsOfDetail.Length - 1].ViewDistanceThreshold;
        m_chunkSize = s_mapGenerator.MapChunkSize() - 1;
        m_chunksVisibleInViewDst = Mathf.RoundToInt(s_maxViewDst / m_chunkSize);
        UpdateVisibleChunks();
    }

    void Update()
    {
        s_viewerPosition = new Vector2(m_viewer.position.x, m_viewer.position.z) / s_mapGenerator.TerrainData.UniformScale;

        if (s_viewerPosition != m_oldViewerPosition)
        {
            foreach (TerrainChunk chunk in s_visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if ((m_oldViewerPosition - s_viewerPosition).sqrMagnitude > SQR_VIEWER_CHUNK_UPDATE_DELTA)
        {
            m_oldViewerPosition = s_viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = s_visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            s_visibleTerrainChunks[i].UpdateTerrainChunk();
            if (i < s_visibleTerrainChunks.Count) // If we didn't end up removing the chunk
                alreadyUpdatedChunkCoords.Add(s_visibleTerrainChunks[i].Coord);
        }

        int currentChunkCoordX = Mathf.RoundToInt(s_viewerPosition.x / m_chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(s_viewerPosition.y / m_chunkSize);

        for (int yOffset = -m_chunksVisibleInViewDst; yOffset <= m_chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -m_chunksVisibleInViewDst; xOffset <= m_chunksVisibleInViewDst; xOffset++)
            {

                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (viewedChunkCoord == Vector2.zero) continue; // Our home chunk is already generated
                if (alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) continue;

                if (s_terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    s_terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    TerrainChunk chunk = new TerrainChunk(viewedChunkCoord, m_chunkSize, m_levelsOfDetail, m_colliderLODIndex, transform, m_mapMaterial);
                    s_terrainChunkDictionary.Add(viewedChunkCoord, chunk);
                    s_visibleTerrainChunks.Add(chunk);
                }

            }
        }
    }

    public class TerrainChunk
    {
        Vector2 m_coord;
        GameObject m_meshObject;
        Vector2 m_position;
        Bounds m_bounds;

        MeshRenderer m_meshRenderer = null;
        MeshFilter m_meshFilter = null;
        MeshCollider m_meshCollider = null;
        LODInfo[] m_detailLevels;
        LODMesh[] m_lodMeshes;
        int m_colliderLODIndex;

        float[,] m_heightMap;
        bool m_heightDataReceived = false, m_hasSetCollider = false;
        int m_prevLODIndex = -1;

        public Vector2 Coord
        {
            get { return m_coord; }
        }

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material)
        {
            m_coord = coord;
            m_detailLevels = detailLevels;
            m_colliderLODIndex = colliderLODIndex;
            m_position = coord * size;
            m_bounds = new Bounds(m_position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(m_position.x, 0, m_position.y);

            m_meshObject = new GameObject("Terrain Chunk");
            m_meshObject.layer = LayerMask.NameToLayer("Terrain");
            m_meshRenderer = m_meshObject.AddComponent<MeshRenderer>();
            m_meshFilter = m_meshObject.AddComponent<MeshFilter>();
            m_meshCollider = m_meshObject.AddComponent<MeshCollider>();
            m_meshRenderer.material = material;

            m_meshObject.transform.position = positionV3 * s_mapGenerator.TerrainData.UniformScale;
            m_meshObject.transform.parent = parent;
            m_meshObject.transform.localScale = Vector3.one * s_mapGenerator.TerrainData.UniformScale;

            m_lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                m_lodMeshes[i] = new LODMesh(detailLevels[i].LOD);
                m_lodMeshes[i].HasUpdated += UpdateTerrainChunk;
                if (i == m_colliderLODIndex)
                    m_lodMeshes[i].HasUpdated += UpdateCollisionMesh;
            }

            s_mapGenerator.RequestMapData(m_position, (heightMap) =>
            {
                m_heightMap = heightMap;
                m_heightDataReceived = true;
                UpdateTerrainChunk();
            });
        }

        public void UpdateTerrainChunk()
        {
            if (!m_heightDataReceived) return;
            float sqrViewerDstFromNearestEdge = m_bounds.SqrDistance(s_viewerPosition);
            bool visible = sqrViewerDstFromNearestEdge <= (s_maxViewDst * s_maxViewDst);
            if (!visible)
            {
                s_visibleTerrainChunks.Remove(this);
                s_terrainChunkDictionary.Remove(m_coord);
                Destroy(m_meshObject);
                return;
            }

            int lodIndex = 0;
            for (int i = 0; i < m_detailLevels.Length - 1; i++)
            {
                if (sqrViewerDstFromNearestEdge > m_detailLevels[i].SqrViewDistanceThreshold)
                    lodIndex = i + 1;
                else
                    break;
            }

            if (lodIndex != m_prevLODIndex)
            {
                LODMesh lodMesh = m_lodMeshes[lodIndex];
                if (lodMesh.HasMesh)
                {
                    m_prevLODIndex = lodIndex;
                    m_meshFilter.mesh = lodMesh.Mesh;
                }
                else if (!lodMesh.HasRequestedMesh)
                    lodMesh.RequestMesh(m_heightMap);
            }
        }

        public void UpdateCollisionMesh()
        {
            if (m_hasSetCollider) return;
            float sqrDistanceToEdge = m_bounds.SqrDistance(s_viewerPosition);

            if (sqrDistanceToEdge < m_detailLevels[m_colliderLODIndex].SqrViewDistanceThreshold)
            {
                if (!m_lodMeshes[m_colliderLODIndex].HasRequestedMesh)
                {
                    m_lodMeshes[m_colliderLODIndex].RequestMesh(m_heightMap);
                }
            }

            if (sqrDistanceToEdge < COLLIDER_GEN_THRESHOLD * COLLIDER_GEN_THRESHOLD)
            {
                if (m_lodMeshes[m_colliderLODIndex].HasMesh)
                {
                    m_meshCollider.sharedMesh = m_lodMeshes[m_colliderLODIndex].Mesh;
                    m_hasSetCollider = true;
                }
            }
        }
    }

    private class LODMesh
    {
        private Mesh m_mesh = null;
        private bool m_hasRequestedMesh = false;
        private bool m_hasMesh = false;

        private int m_lod;

        public event Action HasUpdated;

        public Mesh Mesh
        {
            get { return m_mesh; }
        }

        public bool HasRequestedMesh
        {
            get { return m_hasRequestedMesh; }
        }

        public bool HasMesh
        {
            get { return m_hasMesh; }
        }

        public LODMesh(int lod)
        {
            m_lod = lod;
        }

        public void RequestMesh(float[,] heightMap)
        {
            m_hasRequestedMesh = true;
            s_mapGenerator.RequestMeshData(heightMap, m_lod, (meshData) =>
            {
                m_mesh = meshData.CreateMesh();
                m_hasMesh = true;
                if (HasUpdated != null)
                    HasUpdated();
            });
        }
    }

    [Serializable]
    public struct LODInfo
    {
        [Range(0, MeshGenerator.NUM_SUPPORTED_LOD - 1)]
        public int LOD;
        public float ViewDistanceThreshold;

        public float SqrViewDistanceThreshold
        {
            get { return ViewDistanceThreshold * ViewDistanceThreshold; }
        }
    }
}