using UnityEngine;

namespace IceCracks.Views
{
    public class SimpleMeshView : MonoBehaviour
    {
        [SerializeField] protected MeshFilter meshFilter;
        [SerializeField] protected MeshRenderer meshRenderer;

        protected virtual void Reset() => meshFilter = GetComponent<MeshFilter>();


        public virtual void SetMesh(BMesh mesh)
        {
            meshFilter.mesh = null;
            BMeshUnity.SetInMeshFilter(mesh, meshFilter);
        }

        public MeshRenderer GetMeshRenderer() => meshRenderer;

        public void CacheTexture(Texture2D newTexture)
        {
            meshRenderer.materials[0].mainTexture = newTexture;
        }
    }
}
