using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using Pet.Slime;

namespace IceCracks.CracksGeneration.Models
{
    using Utilities;
    using Interactions;
    using Views;
    using Math;
    public class IcePiece : MonoBehaviour
    {
        [SerializeField] private SimpleMeshViewWithCollider meshView;
        [SerializeField] private MeshCrackGenerator meshCrackGenerator;
        [SerializeField] private MeshCrackVisualizer meshCrackVisualizer;

        private HyperSpace currentData;
        private Bounds lastBoundCut;
        private List<BMesh> cachedBMesh;
        private List<Bounds> bounds = new List<Bounds>();
        private List<HyperSpace> onEdge;
        public bool isNewMesh { get; private set; } = true;

        public void SetupPiece(HyperSpace hyperSpace,Camera raycastCamera)
        {
            onEdge = new List<HyperSpace>();
            currentData = hyperSpace;
            meshView.SetMesh(currentData.GetBMesh());
            meshCrackVisualizer.Initialize(meshCrackGenerator);
            meshCrackGenerator.Initialize(meshCrackVisualizer,raycastCamera);
            meshCrackGenerator.Model.OnNewCoreCreated += CutOffPieces;
            meshCrackGenerator.Model.DetochAllInRange += DetochAllInRange;
        }

        private void OnCrackDrawn()
        {
            meshView.SetMesh(currentData.GetBMesh());
            ReCreateBounds();
        }

        private void OnDestroy()
        {
            meshCrackGenerator.Model.OnNewCoreCreated -= CutOffPieces;
            meshCrackGenerator.Model.DetochAllInRange -= DetochAllInRange;
        }
        private void DetochAllInRange(Bounds obj)
        {
            onEdge.Clear();
            currentData.GetAll(obj, onEdge);
            if (onEdge.Count == 0)
            {
                Debug.LogError("(((");
                return;
            }
            var mesh = HyperSpace.GetMeshFromArea(onEdge, obj);
            onEdge.Clear();

            var item = CrackPiecePool.Instance.GetView();
            Vector2 position = obj.center;
            position = MathExtensions.Rebase(position, -Vector2.one, Vector2.one, -Vector2.one * 5, Vector2.one * 5);
            item.SetMesh(mesh);
            item.GetMeshRenderer().materials = new[] { Instantiate(meshCrackVisualizer.currentMaterial) };
            //item.CacheTexture(meshCrackVisualizer.GetCurrentTextureCopy());

            var swimmingObj = item.IceCrackSwimming;
            swimmingObj.offset = obj.center * -5f + Vector3.forward;
            swimmingObj.size = obj.size.magnitude;
            swimmingObj.numberOfVertex = GetClosestVertexId(position);

            obj.size *= 1.2f;
            onEdge.Clear();
            currentData.CutOut(obj, onEdge);
            HyperSpace.AdjustBorders(onEdge);
            onEdge.Clear();
 
            OnCrackDrawn();
        }
        private async void CutOffPieces(List<BMesh> bMesh,List<Bounds>bounds,Bounds obj)
        {
            this.bounds = bounds;
            isNewMesh = false;
            cachedBMesh = bMesh;
            onEdge.Clear();
            await currentData.CutOut(obj, onEdge);
            HyperSpace.AdjustBorders(onEdge);
            onEdge.Clear();

            //currentData.SplitBySquare(currentData.GetRawSquare() * .15f, onEdge, 2);
            lastBoundCut = obj;
            OnCrackDrawn();
        }

        private async void ReCreateBounds()
        {
            SimpleMeshView item;
            Vector2 position;
            IceCrackSwimming swimmingObj;
            List<SimpleMeshView> items = new List<SimpleMeshView>();
            cachedBMesh.ForEach(_=>items.Add(CrackPiecePool.Instance.GetView()));

            await Task.Yield();
            Material mat = Instantiate(meshCrackVisualizer.currentMaterial);
            await Task.WhenAny(cachedBMesh.Select((c, i) => SetCachedBMesh(c, bounds[i], mat, items[i])));
        }

        private async Task SetCachedBMesh(BMesh mesh,Bounds bounds,Material mat,SimpleMeshView item)
        {
            lastBoundCut = bounds;
            Vector2 position = lastBoundCut.center;
            position = MathExtensions.Rebase(position, -Vector2.one, Vector2.one, -Vector2.one * 5, Vector2.one * 5);
            lastBoundCut.extents *= .98f;
            item.SetMesh(mesh);
            item.GetMeshRenderer().materials = new[] { mat };
            //item.CacheTexture(meshCrackVisualizer.GetCurrentTextureCopy());

            IceCrackSwimming swimmingObj = item.IceCrackSwimming;
            swimmingObj.offset = lastBoundCut.center * -5f + Vector3.forward;
            swimmingObj.size = lastBoundCut.size.magnitude;
            swimmingObj.numberOfVertex = GetClosestVertexId(position);
        }
        

        private int GetClosestVertexId(Vector3 position)
        {
            int idClosest = 0;
            float minDist = Vector3.Distance(position, SwimmingObjectsController.Instance.clothVertices[0]);
            for (int i = 1; i < SwimmingObjectsController.Instance.clothVertices.Length; i++)
            {
                float currentDistance = Vector3.Distance(position, SwimmingObjectsController.Instance.clothVertices[i]);
                if ( currentDistance< minDist)
                {
                    idClosest = i;
                    minDist = currentDistance;
                }
            }
            return idClosest;
        }
        
    }
}
