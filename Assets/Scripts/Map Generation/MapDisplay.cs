using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField]
    private MeshFilter m_meshFilter = null;

    public void DrawMesh(MeshData meshData)
    {
        m_meshFilter.sharedMesh = meshData.CreateMesh();
    }

}
