using IceCracks.CracksGeneration.Models;
using UnityEngine;
using static BMeshUtilities;

public class SplitMeshController : MonoBehaviour
{
    [SerializeField] private IcePiece prefab;
    [SerializeField] private Vector2 size;

    private void Start() => GenerateInitialMesh();

    private void GenerateInitialMesh()
    {
        var piece = Instantiate(prefab);
        piece.SetupPiece(Create(size));
    }
    private HyperSpace Create(Vector2 size)
    {
        return new HyperSpace(size, Vector2.one, -Vector2.one, 10, 4, 0);
    }
}
