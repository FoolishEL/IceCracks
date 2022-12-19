using System.Collections.Generic;
using UnityEngine;

namespace IceCracks.Utilities
{
    using CracksGeneration.Extensions;
    using Math;

    public static class BMeshUtilities
    {
        public static BMesh CreateQuadMesh(Vector2 size, Vector2 topRightPoint, Vector2 bottomLeftPoint)
        {
            BMesh mesh = new BMesh();
            mesh.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);

            List<BMesh.Vertex> vertices = new List<BMesh.Vertex>();
            vertices.Add(AddVertexToMesh(topRightPoint.x, bottomLeftPoint.y, mesh, size));
            vertices.Add(AddVertexToMesh(topRightPoint.x, topRightPoint.y, mesh, size));
            vertices.Add(AddVertexToMesh(bottomLeftPoint.x, topRightPoint.y, mesh, size));
            vertices.Add(AddVertexToMesh(bottomLeftPoint.x, bottomLeftPoint.y, mesh, size));
            mesh.AddFace(vertices.ToArray());
            return mesh;
        }

        public static BMesh CreateMeshFromPoints(List<Vector2> points, Vector2 center,Vector2 size)
        {
            BMesh mesh = new BMesh();
            mesh.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
            List<BMesh.Vertex> vertices = new List<BMesh.Vertex>();
            vertices.Add(AddVertexToMesh(center.x, center.y, mesh, size));
            vertices.Add(AddVertexToMesh(points[0].x, points[0].y, mesh, size));
            vertices.Add(AddVertexToMesh(points[^1].x, points[^1].y, mesh, size));
            mesh.AddFace(vertices.ToArray());
            
            for (int i = 0; i < points.Count - 1; i++)
            {
                BMesh meshTemp = new BMesh();
                meshTemp.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
                List<BMesh.Vertex> verticesTemp = new List<BMesh.Vertex>();
                verticesTemp.Add(AddVertexToMesh(center.x, center.y, meshTemp, size));
                verticesTemp.Add(AddVertexToMesh(points[i+1].x, points[i+1].y, meshTemp, size));
                verticesTemp.Add(AddVertexToMesh(points[i].x, points[i].y, meshTemp, size));
                meshTemp.AddFace(verticesTemp.ToArray());
                BMeshOperators.Merge(mesh, meshTemp);
            }
            return mesh;
        }

        public static List<BMesh> CreateMeshesSegmentsFromPoints(List<Vector2> points, Vector2 center,Vector2 size,List<Bounds> bounds)
        {
            bounds.Clear();
            List<BMesh> meshes = new List<BMesh>();
            if (MathExtensions.GetRandomWithPercent(.3f))
            {
                meshes.Add(CreateVShape(new List<Vector2>() { center, points[0], points[^1] }, size,out var b));
                bounds.Add(b);
            }
            else
            {
                var res = SplitTriangles(center, points[0], points[^1]);
                foreach (var item in res)
                {
                    meshes.Add(CreateVShape(item, size,out var b));
                    bounds.Add(b);
                }
            }
            

            for (int i = 0; i < points.Count - 1; i++)
            {
                if (MathExtensions.GetRandomWithPercent(.2f))
                {
                    meshes.Add(CreateVShape(new List<Vector2>() { center, points[i + 1], points[i] }, size,out var b3));
                    bounds.Add(b3);
                }
                else
                {
                    var res = SplitTriangles(center, points[i + 1], points[i]);
                    foreach (var item in res)
                    {
                        meshes.Add(CreateVShape(item, size,out var b2));
                        bounds.Add(b2);
                    }
                }
            }
            return meshes;
        }

        private static List<List<Vector2>> SplitTriangles(Vector2 center, Vector2 first, Vector2 second)
        {
            List<List<Vector2>> result = new List<List<Vector2>>();
            MathExtensions.SplitFloatByTwo(1f, out var firstD, out var secondD);
            if (firstD > secondD)
                (first, second) = (second, first);
            bool isThree = MathExtensions.GetRandomWithPercent(.5f);

            List<Vector2> current = new List<Vector2>();
            current.Add(center);
            current.Add(Vector2.Lerp(first, center, firstD));
            current.Add(Vector2.Lerp(second, center, firstD));
            current = CrackExtensions.SortVertices(current, true);
            result.Add(current);

            current = new List<Vector2>();
            current.Add(Vector2.Lerp(second, center, firstD));
            current.Add(Vector2.Lerp(second, center, secondD));
            current.Add(Vector2.Lerp(first, center, secondD));
            current.Add(Vector2.Lerp(first, center, firstD));
            current = CrackExtensions.SortVertices(current, true);
            if (TrySplitQuads(current, out var firstL1, out var secondL1))
            {
                result.Add(CrackExtensions.SortVertices(firstL1, true));
                result.Add(CrackExtensions.SortVertices(secondL1, true));
            }
            else
            {
                result.Add(current);
            }
            
            if (isThree)
            {
                current = new List<Vector2>();
                current.Add(Vector2.Lerp( first,center, secondD));
                current.Add(first);
                current.Add(second);
                current.Add(Vector2.Lerp( second,center, secondD));
                current = CrackExtensions.SortVertices(current, true);
                if (TrySplitQuads(current, out var firstL2, out var secondL2))
                {
                    result.Add(CrackExtensions.SortVertices(firstL2, true));
                    result.Add(CrackExtensions.SortVertices(secondL2, true));
                }
                else
                {
                    result.Add(current);
                }
            }
            return result;
        }

        private static bool TrySplitQuads(List<Vector2> initials,out List<Vector2>first,out List<Vector2> second)
        {
            first = new List<Vector2>();
            second = new List<Vector2>();
            if (initials.Count != 4)
            {
                return false;
            }

            if (Vector2.Distance(initials[0], initials[1])*1.3 < Vector2.Distance(initials[2], initials[3]))
            {
                Vector2 firstSplitLine = Vector2.Lerp(initials[0], initials[1], UnityEngine.Random.Range(.4f, .6f));
                Vector2 secondSplitLine = Vector2.Lerp(initials[2], initials[3], UnityEngine.Random.Range(.4f, .6f));
                first.Add(initials[0]);
                first.Add(firstSplitLine);
                first.Add(secondSplitLine);
                first.Add(initials[3]);
                second.Add(initials[1]);
                second.Add(initials[2]);
                second.Add(secondSplitLine);
                second.Add(firstSplitLine);
                return true;
            }

            return false;
        }

        public static BMesh CreateVShape(List<Vector2> points, Vector2 size, out Bounds bound,
            List<int> ignoredSides = null, float defaultLength = .2f)
        {
            float down = -.2f;
            BMesh mesh = new BMesh();
            mesh.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
            List<BMesh.Vertex> vertices = new List<BMesh.Vertex>();
            for (int i = 0; i < points.Count; i++)
            {
                vertices.Add(AddVertexToMesh(points[i].x, points[i].y, mesh, size));
            }
            mesh.AddFace(vertices.ToArray());
            
            Vector2 center = Vector2.zero;
            points.ForEach(c => center += c);
            center /= points.Count;
            bound = new Bounds();
            bound.center = center;
            //List<Vector2> offsetPoints = new List<Vector2>();
            foreach (var p in points)
            {
                //offsetPoints.Add(p + (p - center) * defaultLength);
                bound.Encapsulate(p);
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                if(ignoredSides is not null&& ignoredSides.Contains(i))
                    continue;
                BMeshOperators.Merge(mesh, CreateDownSide(points[i], points[i + 1], center, down, defaultLength, size));
                // BMesh first = new BMesh();
                // first.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
                // vertices = new List<BMesh.Vertex>();
                // vertices.Add(AddVertexToMesh(offsetPoints[i].x, offsetPoints[i].y, first, size,down));
                // vertices.Add(AddVertexToMesh(offsetPoints[i+1].x, offsetPoints[i+1].y, first, size,down));
                // vertices.Add(AddVertexToMesh(points[i+1].x, points[i+1].y, first, size));
                // vertices.Add(AddVertexToMesh(points[i].x, points[i].y, first, size));
                // first.AddFace(vertices.ToArray());
                // BMeshOperators.Merge(mesh, first);
            }

            if (ignoredSides is null || !ignoredSides.Contains(points.Count-1))
            {
                BMeshOperators.Merge(mesh, CreateDownSide(points[^1], points[0], center, down, defaultLength, size));
                // BMesh third = new BMesh();
                // third.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
                // vertices = new List<BMesh.Vertex>();
                // vertices.Add(AddVertexToMesh(offsetPoints[^1].x, offsetPoints[^1].y, third, size, down));
                // vertices.Add(AddVertexToMesh(offsetPoints[0].x, offsetPoints[0].y, third, size, down));
                // vertices.Add(AddVertexToMesh(points[0].x, points[0].y, third, size));
                // vertices.Add(AddVertexToMesh(points[^1].x, points[^1].y, third, size));
                // third.AddFace(vertices.ToArray());
                //
                // BMeshOperators.Merge(mesh, third);
            }

            return mesh;
        }

        public static BMesh CreateDownSide(Vector2 startPoint, Vector2 endPoint, Vector2 linesFrom, float depth,
            float offset,Vector2 size)
        {
            Vector2 startDowned = startPoint + (startPoint - linesFrom) * offset;
            Vector2 endDowned = endPoint + (endPoint - linesFrom) * offset;
            BMesh first = new BMesh();
            first.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
            List<BMesh.Vertex> vertices = new List<BMesh.Vertex>();
            vertices.Add(AddVertexToMesh(startDowned.x, startDowned.y, first, size,depth));
            vertices.Add(AddVertexToMesh(endDowned.x, endDowned.y, first, size,depth));
            vertices.Add(AddVertexToMesh(endPoint.x, endPoint.y, first, size));
            vertices.Add(AddVertexToMesh(startPoint.x, startPoint.y, first, size));
            first.AddFace(vertices.ToArray());
            
            return first;
        }
        
        private static BMesh.Vertex AddVertexToMesh(float x, float y, BMesh bMesh, Vector2 size, float height = 0f)
        {
            float initialX = (x + 1f) / 2f;
            float initialY = (1f - y) / 2f;
            x *= (size.x / 2f);
            y *= (size.y / 2f);
            var vert = bMesh.AddVertex(x, height, y);
            vert.attributes["uv"] = new BMesh.FloatAttributeValue(initialX, initialY);
            return vert;
        }


        #region BoundsExtensions
        public static Vector2 TopLeft(this Bounds bounds) => new Vector2(bounds.min.x, bounds.max.y);

        public static Vector2 BottomRight(this Bounds bounds) => new Vector2(bounds.max.x, bounds.min.y);

        public static Vector2 TopRight(this Bounds bounds) => bounds.max;

        public static Vector2 BottomLeft(this Bounds bounds) => bounds.min;

        public static Vector2 OnLeft(this Bounds bounds, float p) =>
            Vector2.Lerp(TopLeft(bounds), BottomLeft(bounds), p);

        public static Vector2 OnRight(this Bounds bounds, float p) =>
            Vector2.Lerp(TopRight(bounds), BottomRight(bounds), p);

        public static Vector2 OnTop(this Bounds bounds, float p) =>
            Vector2.Lerp(TopLeft(bounds), TopRight(bounds), p);

        public static Vector2 OnBottom(this Bounds bounds, float p) =>
            Vector2.Lerp(BottomLeft(bounds), BottomRight(bounds), p);



        #endregion

    }
}