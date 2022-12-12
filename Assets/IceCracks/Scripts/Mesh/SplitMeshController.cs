using IceCracks.CracksGeneration.Models;
using UnityEngine;
using static BMeshUtilities;

public class SplitMeshController : MonoBehaviour
{
    [SerializeField] private IcePiece prefab;
    [SerializeField] private Vector2 size;
    
    [SerializeField]
    private int spiltCount = 6;
    [SerializeField]
    private int depth = 4;

    private void Start() => GenerateInitialMesh();

    private void GenerateInitialMesh()
    {
        var piece = Instantiate(prefab);
        piece.SetupPiece(Create(size));
    }
    private HyperSpace Create(Vector2 size)
    {
        return new HyperSpace(size, Vector2.one, -Vector2.one, spiltCount, depth, 0);
    }
    
}
