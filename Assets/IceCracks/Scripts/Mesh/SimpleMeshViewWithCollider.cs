using UnityEngine;

namespace IceCracks.Views
{
    [RequireComponent(typeof(MeshCollider))]
    public class SimpleMeshViewWithCollider : SimpleMeshView
    {
        [SerializeField] private MeshCollider meshCollider;
        
        protected override void Reset()
        {
            base.Reset();
            meshCollider = GetComponent<MeshCollider>();
        }

        public override void SetMesh(BMesh mesh)
        {
            base.SetMesh(mesh);
            meshCollider.sharedMesh = meshFilter.mesh;
        }
    }
}
