using System.Collections.Generic;
using Pet.Slime;
using UnityEngine;

namespace IceCracks.CracksGeneration.Models
{
    using static Utilities.BMeshUtilities;
    using Interactions;
    using Views;
    using Math;
    public class IcePiece : MonoBehaviour
    {
        [SerializeField] private SimpleMeshViewWithCollider meshView;
        [SerializeField] private MeshCrackGenerator meshCrackGenerator;
        [SerializeField] private MeshCrackVisualizer meshCrackVisualizer;
        [SerializeField] private List<float> percents;

        private HyperSpace currentData;
        private bool isGenerated = false;
        private Bounds lastBoundCut;
        private BMesh cachedBMesh;
        private Vector2 size;
        private List<HyperSpace> onEdge;

        public void Stop()
        {
            meshCrackGenerator.SetBusyStatus(new object());
        }

        public void SetupPiece(HyperSpace hyperSpace,Vector2 size,Camera raycastCamera)
        {
            onEdge = new List<HyperSpace>();
            currentData = hyperSpace;
            meshView.SetMesh(currentData.GetBMesh());
            meshCrackVisualizer.Initialize(meshCrackGenerator);
            meshCrackGenerator.Initialize(meshCrackVisualizer,raycastCamera);
            this.size = size;
            meshCrackGenerator.Model.OnNewCoreCreated += CutOffPieces;
            meshCrackVisualizer.OnCracksDrawn += OnCrackDrawn;
        }

        private void OnCrackDrawn()
        {
            if (!isGenerated)
                return;
            meshView.SetMesh(currentData.GetBMesh());
            ReCreateBounds();
        }

        private void OnDestroy()
        {
            meshCrackGenerator.Model.OnNewCoreCreated -= CutOffPieces;
            meshCrackVisualizer.OnCracksDrawn -= OnCrackDrawn;
        }

        private async void CutOffPieces(BMesh bMesh,Bounds obj)
        {
            cachedBMesh = bMesh;
            isGenerated = false;
            meshCrackGenerator.SetBusyStatus(this);
            obj.size *= 1.05f;
            onEdge.Clear();
            await currentData.CutOut(obj, onEdge);
            HyperSpace.AdjustBorders(onEdge);
            onEdge.Clear();
            for (int i = 0; i < percents.Count; i++)
            {
                if (percents[i] > 0f && percents[i] < 1f)
                {
                    currentData.SplitBySquare(currentData.GetRawSquare() * percents[i], onEdge, i + 1);
                }
            }
            
            //currentData.SplitBySquare(currentData.GetRawSquare() * .15f, onEdge, 2);
            lastBoundCut = obj;
            isGenerated = true;
            if (meshCrackVisualizer.IsDrawn)
            {
                OnCrackDrawn();
            }
        }

        private void ReCreateBounds()
        {
            var item = CrackPiecePool.Instance.GetView();
            Vector2 position = lastBoundCut.center;
            position = MathExtensions.Rebase(position, -Vector2.one, Vector2.one, -Vector2.one * 5, Vector2.one * 5);
            lastBoundCut.extents *= .98f;
            item.SetMesh(cachedBMesh);
            item.GetMeshRenderer().materials = new[] { Instantiate(meshCrackVisualizer.currentMaterial) };
            item.CacheTexture(meshCrackVisualizer.GetCurrentTextureCopy());
            
            var swimmingObj = item.gameObject.GetComponent<IceCrackSwimming>();
            swimmingObj.offset = lastBoundCut.center * -5f + Vector3.forward;
            swimmingObj.size = lastBoundCut.size.magnitude;
            swimmingObj.numberOfVertex = GetClosestVertexId(position);
            
            if (onEdge.Count > 0)
            {
                foreach (var hS in onEdge)
                {
                    item = CrackPiecePool.Instance.GetView();
                    position = hS.GetMainSquarePosition();
                    position = MathExtensions.Rebase(position, -Vector2.one, Vector2.one, -Vector2.one * 5, Vector2.one * 5);
                    item.SetMesh(hS.GetBMesh(true));
                    item.GetMeshRenderer().materials = new[] { Instantiate(meshCrackVisualizer.currentMaterial) };
                    item.CacheTexture(meshCrackVisualizer.GetCurrentTextureCopy());
            
                    swimmingObj = item.gameObject.GetComponent<IceCrackSwimming>();
                    swimmingObj.size = hS.GetRawSquare();
                    swimmingObj.offset = hS.GetMainSquarePosition() * -5f + Vector3.forward;
                    swimmingObj.numberOfVertex = GetClosestVertexId(position);
                }
                onEdge.Clear();
            }
            meshCrackGenerator.UnsetBusyStatus(this);
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
