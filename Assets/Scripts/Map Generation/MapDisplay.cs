using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField]
    private MeshFilter m_meshFilter = null;

    public void DrawMesh(MeshData meshData, float scale)
    {
        m_meshFilter.transform.localScale = Vector3.one * scale;
        m_meshFilter.sharedMesh = meshData.CreateMesh();
    }

}
