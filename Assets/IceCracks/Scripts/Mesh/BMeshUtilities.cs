using System.Collections.Generic;
using UnityEngine;
using static BMesh;

public static class BMeshUtilities 
{
    public static BMesh CreateQuadMesh(Vector2 size,Vector2 topLeftPoint, Vector2 bottomRightPoint )
    {
        BMesh mesh = new BMesh();
        var uv = mesh.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
        
        List<Vertex> vertices = new List<Vertex>();
        vertices.Add(AddVertexToMesh(bottomRightPoint.x, bottomRightPoint.y, mesh, size));
        vertices.Add(AddVertexToMesh(bottomRightPoint.x, topLeftPoint.y, mesh, size));
        vertices.Add(AddVertexToMesh(topLeftPoint.x, topLeftPoint.y, mesh, size));
        vertices.Add(AddVertexToMesh(topLeftPoint.x, bottomRightPoint.y, mesh, size));
        mesh.AddFace(vertices.ToArray());
        return mesh;
    }

    private static Vertex AddVertexToMesh(float x,float y,BMesh bMesh,Vector2 size)
    {
        float initialX = (x + 1f) / 2f;
        float initialY = (y + 1f) / 2f;
        x *= (size.x / 2f);
        y *= (size.y / 2f);
        var vert = bMesh.AddVertex(x, 0, y);
        vert.attributes["uv"] = new BMesh.FloatAttributeValue(initialX, initialY);
        return vert;
    }
    
    private struct Polygon
    {
        public List<Vector2> positions;
        
    }
}
