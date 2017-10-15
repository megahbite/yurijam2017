using UnityEngine;

public static class MeshGenerator
{
    public const int NUM_SUPPORTED_LOD = 5;
    public const int NUM_SUPPORTED_CHUNK_SIZES = 9;
    public static readonly int[] SupportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail)
    {
        AnimationCurve _heightCurve = new AnimationCurve(heightCurve.keys);
        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2;

        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;


        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine);

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                int vertexIndex = vertexIndicesMap[x, y];
                Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                float height = _heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + meshSimplificationIncrement, y];
                    int c = vertexIndicesMap[x, y + meshSimplificationIncrement];
                    int d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];
                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }

                vertexIndex++;
            }
        }

        meshData.BakeNormals();

        return meshData;

    }
}

public class MeshData
{
    Vector3[] m_vertices;
    int[] m_triangles;
    Vector2[] m_uvs;
    Vector3[] m_bakedNormals;

    Vector3[] m_borderVertices;
    int[] m_borderTriangles;

    int m_triangleIndex;
    int m_borderTriangleIndex;

    public MeshData(int verticesPerLine)
    {
        m_vertices = new Vector3[verticesPerLine * verticesPerLine];
        m_uvs = new Vector2[verticesPerLine * verticesPerLine];
        m_triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        m_borderVertices = new Vector3[verticesPerLine * 4 + 4];
        m_borderTriangles = new int[24 * verticesPerLine];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            m_borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            m_vertices[vertexIndex] = vertexPosition;
            m_uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            m_borderTriangles[m_borderTriangleIndex] = a;
            m_borderTriangles[m_borderTriangleIndex + 1] = b;
            m_borderTriangles[m_borderTriangleIndex + 2] = c;
            m_borderTriangleIndex += 3;
        }
        else
        {
            m_triangles[m_triangleIndex] = a;
            m_triangles[m_triangleIndex + 1] = b;
            m_triangles[m_triangleIndex + 2] = c;
            m_triangleIndex += 3;
        }
    }

    Vector3[] CalculateNormals()
    {

        Vector3[] vertexNormals = new Vector3[m_vertices.Length];
        int triangleCount = m_triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = m_triangles[normalTriangleIndex];
            int vertexIndexB = m_triangles[normalTriangleIndex + 1];
            int vertexIndexC = m_triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = m_borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = m_borderTriangles[normalTriangleIndex];
            int vertexIndexB = m_borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = m_borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }


        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;

    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? m_borderVertices[-indexA - 1] : m_vertices[indexA];
        Vector3 pointB = (indexB < 0) ? m_borderVertices[-indexB - 1] : m_vertices[indexB];
        Vector3 pointC = (indexC < 0) ? m_borderVertices[-indexC - 1] : m_vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void BakeNormals()
    {
        m_bakedNormals = CalculateNormals();
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = m_vertices;
        mesh.triangles = m_triangles;
        mesh.uv = m_uvs;
        mesh.normals = m_bakedNormals;
        return mesh;
    }
}