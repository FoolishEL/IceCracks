using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    private Mesh _mesh;
    private Vector2 _startPos;
    private Vector2 _endPos;
    private List<Vector3> _points;

    public MeshData(Vector2 startPos, Vector2 endPos, IEnumerable<Vector3> points)
    {
        _mesh = new Mesh {
            name = "Slice"
        };
        _startPos = startPos;
        _endPos = endPos;
        _points = new List<Vector3>();
        _points.AddRange(points);
    }

    private struct Triangle
    {
        public Vector3 firstPoint;
        public Vector3 secondPoint;
        public Vector3 thirdPoint;
    }
}
