using UnityEngine;

[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
public class SimpleMeshView : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;

    private void Reset() => meshFilter = GetComponent<MeshFilter>();

    public void SetMesh(Mesh mesh)
    {
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public Mesh GetMesh() => meshFilter.mesh;

}
