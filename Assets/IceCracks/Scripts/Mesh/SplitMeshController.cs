using UnityEngine;
using static BMesh;

public class SplitMeshController : MonoBehaviour
{
    [SerializeField] private SimpleMeshView prefab;
    [SerializeField] private Vector2 size;
    [SerializeField] private float resolution;
    
    //Obsolete
   /* 
    private List<Vector3> vertices;
    private List<Vector2> uvs;
    private List<int> triangles;
    */

    private void Awake()
    {
        GenerateInitialMesh();
    }

    private void GenerateInitialMesh()
    {
        var mView = Instantiate(prefab);
        mView.SetMesh(Create(size));
    }
    //Obsolete
    /*
    [Obsolete]
    private Mesh CreateDefaultMesh(Vector2 size, int resolution)
    {
        var mesh = new Mesh {
            name = "Procedural Mesh"
        };
        vertices = new();
        uvs = new();
        
        float xPerStep = size.x / resolution;
        float yPerStep = size.y / resolution;
        
        for (int y = 0; y < resolution + 1; y++)
        {
            for (int x = 0; x < resolution + 1; x++)
            {
                vertices.Add(new Vector3(x * xPerStep, 0, y * yPerStep));
                uvs.Add(new Vector2((x * xPerStep / size.x), y * yPerStep / size.y));
            }
        }

        triangles = new();
        for (int row = 0; row < resolution; row++)
        {
            for (int column = 0; column < resolution; column++)
            {
                int i = row * resolution + row + column;
                triangles.Add(i);
                triangles.Add(i + resolution + 1);
                triangles.Add(i + resolution + 2);
                
                triangles.Add(i);
                triangles.Add(i + resolution + 2);
                triangles.Add(i + 1);
            }
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        // mesh.RecalculateTangents();
        return mesh;
    }
    */

    private BMesh Create(Vector2 size)
    {
        BMesh f1 = BMeshUtilities.CreateQuadMesh(size, new Vector2(-1, -1f / 3f), new Vector2(1, -1));
        BMesh f2 = BMeshUtilities.CreateQuadMesh(size, new Vector2(1f / 3f, 1f / 3f), new Vector2(1, -1f / 3f));
        BMesh f3 = BMeshUtilities.CreateQuadMesh(size, new Vector2(-1, 1f / 3f), new Vector2(-1f/3f, -1f/3f));
        BMesh f4 = BMeshUtilities.CreateQuadMesh(size, new Vector2(-1, 1f), new Vector2(1, 1f/3f));
        BMeshOperators.Merge(f1, f2);
        BMeshOperators.Merge(f1, f3);
        BMeshOperators.Merge(f1, f4);
        // Set the current mesh filter to use our generated mesh
        return f1;
    }
    

    private void SplitMesh(SimpleMeshView simpleMeshView)
    {
        var initialMesh = simpleMeshView.GetMesh();
        
    }
    
}
