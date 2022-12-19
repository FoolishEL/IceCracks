using System.Collections.Generic;
using System.Linq;
using IceCracks.Utilities;
using Jobberwocky.GeometryAlgorithms.Examples.Data;
using Jobberwocky.GeometryAlgorithms.Extrusion;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private MeshFilter mf;
    [SerializeField] private MeshCollider meshCollider;
    [SerializeField,Min(.1f)] private float stepSize = .5f;
    [SerializeField, Min(1)] private int iterationsCount = 10;

    [SerializeField] private Vector2 position;
    [SerializeField] private Vector2 size;
    
    private readonly TriangulationAPI triangulationApi = new TriangulationAPI();
    private ExtrusionAlgorithm extrusionAlgorithm => ExtrusionAlgorithm.Instance;
    private List<Mesh> meshes;
    private int id = 0;
    private void Awake()
    {
        meshes = new List<Mesh>();
        BMesh bmesh = BMeshUtilities.CreateQuadMesh(Vector2.one * 10f, new Vector2(1f, 1f), new Vector2(-1f, -1f));
        BMeshUnity.SetInMeshFilter(bmesh, mf);
    }
    

    private void FirstTest()
    {
        List<Vector3[]> holes = new List<Vector3[]>();
        List<Vector3> hole = new List<Vector3>();
        hole.Add(new Vector3(-.5f,-.5f));
        hole.Add(new Vector3(-.5f,1.5f));
        hole.Add(new Vector3(.5f,1.5f));
        hole.Add(new Vector3(.5f,-.5f));
        holes.Add(hole.ToArray());
        hole = new List<Vector3>();
        hole.Add(new Vector3(-.5f,-.5f));
        hole.Add(new Vector3(-.5f,.5f));
        hole.Add(new Vector3(1.5f,.5f));
        hole.Add(new Vector3(1.5f,-.5f));
        holes.Add(hole.ToArray());
        var parameters = new Triangulation2DParameters();
        parameters.Boundary = GetBounds();
        parameters.Side = Side.Back;
        parameters.Holes = holes.ToArray();
        parameters.Delaunay = false;
        var triangulationAPI = new TriangulationAPI();
        var mesh = triangulationAPI.Triangulate2D(parameters);
        var raw = triangulationAPI.Triangulate2DRaw(parameters);
        SetUVsToMesh(mesh);
        mf.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private void SetUVsToMesh(Mesh mesh)
    {
        List<Vector2> uvs = new List<Vector2>();
        Vector2 max = mesh.bounds.max - mesh.bounds.min;
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            Vector2 vert = mesh.vertices[i] - mesh.bounds.min;
            uvs.Add(new Vector2(vert.x / max.x, vert.y / max.y));
        }
        mesh.uv = uvs.ToArray();
    }

    private Shape CreateSquareFromPositionAndSize(Vector2 position, Vector2 size)
    {
        List<Vector3> bounds = new List<Vector3>();
        position -= size / 2;
        bounds.Add(position);
        bounds.Add(position+Vector2.up*size.y);
        bounds.Add(position + Vector2.up * size.y + Vector2.right * size.x);
        bounds.Add(position + Vector2.right * size.x);
        Shape shape = new Shape
        {
            Boundary = bounds.ToArray()
        };
        //shape.
        return shape;
    }

    private Mesh GetTriangulatedMeshFromShape(Shape shape)
    {
        var parameters = new Triangulation2DParameters();
        parameters.Points = shape.Points;
        parameters.Boundary = shape.Boundary;
        parameters.Holes = shape.Holes;
        parameters.Delaunay = true;

        return triangulationApi.Triangulate2D(parameters);
    }
    

    private Vector3[] GetBounds()
    {
        List<Vector2> bounds = new List<Vector2>();
        List<Vector2> directions = new List<Vector2> { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
        Vector2 startPoint = Vector2.zero - directions[0] * stepSize * iterationsCount / 2 -
                             directions[1] * stepSize * iterationsCount / 2;
        for (int i = 0; i < directions.Count; i++)
        {
            for (int j = 0; j < iterationsCount; j++)
            {
                bounds.Add(startPoint);
                startPoint += directions[i] * stepSize;
            }
        }
        
        return bounds.Select(c=>(Vector3)c).ToArray();
    }
    
}
