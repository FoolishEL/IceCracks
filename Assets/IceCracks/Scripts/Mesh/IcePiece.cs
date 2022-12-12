using System;
using UnityEngine;
using static BMeshUtilities;

namespace IceCracks.CracksGeneration.Models
{
    public class IcePiece : MonoBehaviour
    {
        [SerializeField] private SimpleMeshView meshView;
        [SerializeField] private MeshCrackGenerator meshCrackGenerator;
        [SerializeField] private MeshCrackVisualizer meshCrackVisualizer;
        
        private HyperSpace currentData;
        
        public void SetupPiece(HyperSpace hyperSpace)
        {
            currentData = hyperSpace;
            meshView.SetMesh(currentData.GetBMesh());
            meshCrackVisualizer.Initialize();
            meshCrackGenerator.Initialize();
            meshCrackGenerator.Model.OnNewCoreCreated += CutOffPieces;
        }

        private void OnDestroy()
        {
            meshCrackGenerator.Model.OnNewCoreCreated -= CutOffPieces;
        }

        private async void CutOffPieces(Rectangle obj)
        {
            float time = Time.time;
            await currentData.CutOut(obj);
            Debug.LogError($"Data created in {Time.time -time} seconds");
            meshView.SetMesh(currentData.GetBMesh());
        }
    }
}
