using UnityEngine;

[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
public class SimpleMeshView : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;

    private void Reset() => meshFilter = GetComponent<MeshFilter>();
    

    public void SetMesh(BMesh mesh)
    {
        meshFilter.mesh = null;
        BMeshUnity.SetInMeshFilter(mesh, meshFilter);
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    public Mesh GetMesh() => meshFilter.mesh;

}
