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
        //TODO: what is 2 means? parametrize this! 
        return new HyperSpace(size, Vector2.zero, Vector2.one*2 , spiltCount, depth, 0);
    }
    
}
