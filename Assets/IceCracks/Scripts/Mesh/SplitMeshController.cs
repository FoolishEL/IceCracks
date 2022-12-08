using UnityEngine;
using static BMeshUtilities;

public class SplitMeshController : MonoBehaviour
{
    [SerializeField] private SimpleMeshView prefab;
    [SerializeField] private Vector2 size;
    [SerializeField] private float resolution;

    public static Vector2 Size { get;private set; }
    
    private void Awake()
    {
        Size = size;
        GenerateInitialMesh();
    }

    private void GenerateInitialMesh()
    {
        var mView = Instantiate(prefab);
        mView.SetMesh(Create(size));
    }
    private BMesh Create(Vector2 size)
    {
        HyperSpace space = new HyperSpace(size, Vector2.one, -Vector2.one, 4, 6, 0);
        return space.GetBMesh();
    }
    

    private void SplitMesh(SimpleMeshView simpleMeshView)
    {
        var initialMesh = simpleMeshView.GetMesh();
        
    }
    
}
