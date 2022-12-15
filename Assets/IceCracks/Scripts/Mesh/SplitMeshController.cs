using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IceCracks.CracksGeneration
{
    using Views;
    using static Utilities.BMeshUtilities;
    using Models;
    
    public class SplitMeshController : MonoBehaviour
    {
        [SerializeField] private IcePiece prefab;
        [SerializeField] private Vector2 size;
        [SerializeField] private Camera raycastCamera;

        [SerializeField] private List<int> splitCountByDepth;
        private IcePiece lastPiece;

        public static event Action OnRestartCommandRecive = delegate { };

        [ContextMenu(nameof(KKk))]
        private void KKk()
        {
            SceneManager.LoadScene(0);
        }
        private void Start() => GenerateInitialMesh();

        private void GenerateInitialMesh()
        {
            var piece = Instantiate(prefab);
            piece.transform.position = transform.position;
            piece.SetupPiece(Create(), size, raycastCamera);
            lastPiece = piece;
        }

        private HyperSpace Create()
        {
            //TODO: what is 2 means? parametrize this! 
            splitAmounts = new List<int>();
            splitAmounts.AddRange(splitCountByDepth);

            return new HyperSpace(size, Vector2.zero, Vector2.one * 2, splitCountByDepth.Count, 0);
        }

        public static IReadOnlyList<int> SplitAmounts => splitAmounts;
        private static List<int> splitAmounts;
        private bool isAppeared;

        [ContextMenu(nameof(Restart))]
        public async void Restart()
        {
            isAppeared = false;
            MeshCrackVisualizer.OnAppear += OnMeshAppear;
            lastPiece.Stop();
            var piece = Instantiate(prefab);
            piece.transform.position = transform.position;
            piece.SetupPiece(Create(), size, raycastCamera);
            while (!isAppeared)
            {
                await Task.Yield();
            }
            OnRestartCommandRecive.Invoke();
            Destroy(lastPiece.gameObject);
            lastPiece = piece;
        }

        private void OnMeshAppear()
        {
            MeshCrackVisualizer.OnAppear -= OnMeshAppear;
            isAppeared = true;
        }
    }
}