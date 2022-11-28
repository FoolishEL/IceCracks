using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class MeshView : MonoBehaviour
{
    [SerializeField] private Vector2 size;
    [SerializeField] private int resolution;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private List<Vector3> vertices;
    private List<Vector2> uvs;
    private List<int> triangles;

    [ContextMenu(nameof(GenerateMesh))]
    private void GenerateMesh()
    {
        PrepareView();
        meshFilter.mesh = CreateMesh(size, resolution);
    }
    
    private Mesh CreateMesh(Vector2 size, int resolution)
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

    private void PrepareView()
    {
        meshFilter ??= GetComponent<MeshFilter>();
        meshRenderer ??= GetComponent<MeshRenderer>();
    }
}
