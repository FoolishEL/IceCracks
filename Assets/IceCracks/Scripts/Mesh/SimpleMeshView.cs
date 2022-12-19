using UnityEngine;

namespace IceCracks.Views
{
    using Interactions;
    public class SimpleMeshView : MonoBehaviour
    {
        [SerializeField] protected MeshFilter meshFilter;
        [SerializeField] protected MeshRenderer meshRenderer;
        [SerializeField] private IceCrackSwimming iceCrackSwimming;
        public IceCrackSwimming IceCrackSwimming => iceCrackSwimming;

        protected virtual void Reset() => meshFilter = GetComponent<MeshFilter>();


        public virtual void SetMesh(BMesh mesh)
        {
            meshFilter.mesh = null;
            BMeshUnity.SetInMeshFilter(mesh, meshFilter);
        }

        public void SetRawMesh(Mesh mesh)
        {
            meshFilter.mesh = mesh;
        }

        public MeshRenderer GetMeshRenderer() => meshRenderer;

        // public void CacheTexture(Texture2D newTexture)
        // {
        //     meshRenderer.materials[0].mainTexture = newTexture;
        // }
    }
}
