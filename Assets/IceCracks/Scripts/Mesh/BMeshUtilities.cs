using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IceCracks.CracksGeneration.Extensions;
using IceCracks.Math;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using UnityEngine;
using Random = System.Random;

namespace IceCracks.Utilities
{
    using CracksGeneration;

    public static class BMeshUtilities
    {
        public static BMesh CreateQuadMesh(Vector2 size, Vector2 topRightPoint, Vector2 bottomLeftPoint)
        {
            BMesh mesh = new BMesh();
            var uv = mesh.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);

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
            var uv = mesh.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
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

        public static BMesh CreateVShape(List<Vector2> points,Vector2 size,out Bounds bound)
        {
            float down = -.2f;
            BMesh mesh = new BMesh();
            var uv = mesh.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
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
            List<Vector2> offsetPoints = new List<Vector2>();
            foreach (var p in points)
            {
                offsetPoints.Add(p + (p - center) * .2f);
                bound.Encapsulate(p);
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                BMesh first = new BMesh();
                uv = first.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
                vertices = new List<BMesh.Vertex>();
                vertices.Add(AddVertexToMesh(offsetPoints[i].x, offsetPoints[i].y, first, size,down));
                vertices.Add(AddVertexToMesh(offsetPoints[i+1].x, offsetPoints[i+1].y, first, size,down));
                vertices.Add(AddVertexToMesh(points[i+1].x, points[i+1].y, first, size));
                vertices.Add(AddVertexToMesh(points[i].x, points[i].y, first, size));
                first.AddFace(vertices.ToArray());
                BMeshOperators.Merge(mesh, first);
            }

            BMesh third = new BMesh();
            uv = third.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
            vertices = new List<BMesh.Vertex>();
            vertices.Add(AddVertexToMesh(offsetPoints[^1].x, offsetPoints[^1].y, third, size,down));
            vertices.Add(AddVertexToMesh(offsetPoints[0].x, offsetPoints[0].y, third, size,down));
            vertices.Add(AddVertexToMesh(points[0].x, points[0].y, third, size));
            vertices.Add(AddVertexToMesh(points[^1].x, points[^1].y, third, size));
            third.AddFace(vertices.ToArray());
            
            BMeshOperators.Merge(mesh, third);
            return mesh;
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

        public class HyperSpace
        {
            private Bounds mainSquare;
            private readonly Vector2 size;
            private readonly int splitCount;
            private readonly int maxDepth;
            private readonly int currentDepth;

            private bool isEmpty;
            private bool isCut;
            private HyperSpace[,] subSpaces;
            private List<HyperSpace> listedSubSpaces;
            // private List<Bounds> emptyBy;
            // private List<Bounds> cutBy;

            private BMesh cachedBMesh;
            private bool isCorner;

            private float cachedSquare;

            public HyperSpace(Vector2 size, Vector2 boundsCenter, Vector2 boundsSize, int maxDepth,
                int currentDepth)
            {
                this.size = size;
                splitCount = SplitMeshController.SplitAmounts.Count - 1 > currentDepth
                    ? SplitMeshController.SplitAmounts[currentDepth]
                    : SplitMeshController.SplitAmounts[0];
                this.maxDepth = maxDepth;
                this.currentDepth = currentDepth;
                isCorner = false;
                // emptyBy = new List<Bounds>();
                // cutBy = new List<Bounds>();
                isEmpty = false;
                isCut = false;
                mainSquare = new Bounds
                {
                    center = boundsCenter,
                    size = boundsSize
                };
                if (currentDepth == maxDepth)
                    cachedSquare = GetRawSquare();
                else
                    cachedSquare = -1f;
                
                if (currentDepth == 0)
                {
                    GenerateSubSpaces();
                }
            }

            public float GetRawSquare() => mainSquare.extents.magnitude;

            public float GetSquare()
            {
                if (cachedSquare > 0f)
                    return cachedSquare;
                if (isEmpty)
                {
                    cachedSquare = 0f;
                    return 0f;
                }

                if (isCut)
                {
                    cachedSquare = 0f;
                    foreach (var item in subSpaces)
                    {
                        if (item.isEmpty)
                            continue;
                        cachedSquare += item.GetSquare();
                    }
                }
                else
                {
                    cachedSquare = GetRawSquare();
                }
                return cachedSquare;
            }

            public Vector3 GetMainSquarePosition() => mainSquare.center;

            public async Task CutOut(Bounds rectangleOrdinary,List<HyperSpace>onEdge)
            {
                if (!mainSquare.Intersects(rectangleOrdinary)||isEmpty)
                {
                    return;
                }

                if (currentDepth >= maxDepth ||
                    (rectangleOrdinary.Contains(mainSquare.max) && rectangleOrdinary.Contains(mainSquare.min)))
                {
                    if (currentDepth >= maxDepth)
                    {
                        if (Vector2.Distance(mainSquare.center, rectangleOrdinary.center) > rectangleOrdinary.size.magnitude*.4f)
                        {
                            onEdge.Add(this);
                            return;
                        }
                    }
                    //empty.Add(this);
                    //emptyBy.Add(rectangleOrdinary);
                    cachedBMesh = null;
                    isEmpty = true;
                    return;
                }

                cachedBMesh = null;
                if (subSpaces is null)
                    GenerateSubSpaces();
                isCut = true;
                cachedSquare = -1f;
                //cutBy.Add(rectangleOrdinary);
                int emptinessCount = 0;
                //await Task.WhenAll(listedSubSpaces.Select(c => c.CutOut(rectangle)));

                #region unuptimized

                for (int i = 0; i < splitCount; i++)
                {
                    for (int j = 0; j < splitCount; j++)
                    {
                        var hspace = subSpaces[i, j];
                        await hspace.CutOut(rectangleOrdinary, onEdge);
                        if (hspace.isEmpty)
                            emptinessCount++;
                    }
                }

                /*
                foreach (var hspace in subSpaces)
                {
                    await Task.Yield();
                    await hspace.CutOut(rectangle);
                    if (hspace.isEmpty)
                    {
                        emptinessCount++;
                        Debug.LogError($"Cutted out {}");
                    }
                }
                */

                #endregion

                //emptinessCount = listedSubSpaces.Count(c => c.isEmpty);
                if (emptinessCount == splitCount * splitCount)
                {
                    // if (cutBy.Count > 1)
                    // {
                    //     emptyBy.AddRange(cutBy);
                    // }
                    // else
                    // {
                    //     emptyBy.Add(rectangleOrdinary);
                    // }

                    isEmpty = true;
                    cachedBMesh = null;
                }
            }

            public async Task GetAll(Bounds bounds,List<BMesh> meshes)
            {
                if (!mainSquare.Intersects(bounds)||isEmpty)
                {
                    return;
                }
                // if (currentDepth >= maxDepth Vector2.Distance(mainSquare.center, bounds.center) > bounds.size.magnitude*.4f)
                // {
                //     meshes.Add(GetBMesh());
                //     cachedBMesh = null;
                //     isEmpty = true;
                //     return;
                // }

                if (currentDepth >= maxDepth ||
                    (bounds.Contains(mainSquare.max) && bounds.Contains(mainSquare.min))) 
                {
                    meshes.Add(GetBMesh());
                    cachedBMesh = null;
                    isEmpty = true;
                    return;
                }

                cachedBMesh = null;
                if (subSpaces is null)
                    GenerateSubSpaces();
                isCut = true;
                cachedSquare = -1f;
                //cutBy.Add(rectangleOrdinary);
                int emptinessCount = 0;
                //await Task.WhenAll(listedSubSpaces.Select(c => c.CutOut(rectangle)));

                #region unuptimized

                for (int i = 0; i < splitCount; i++)
                {
                    for (int j = 0; j < splitCount; j++)
                    {
                        var hspace = subSpaces[i, j];
                        await hspace.GetAll(bounds, meshes);
                        if (hspace.isEmpty)
                            emptinessCount++;
                    }
                }

                #endregion
                
                if (emptinessCount == splitCount * splitCount)
                {
                    isEmpty = true;
                    cachedBMesh = null;
                }
                
            }

            public static void AdjustBorders(List<HyperSpace> edges)
            {
                if(edges is null|| edges.Count==0)
                    return;
                var firstEdge = edges[0];
                List<(Vector2Int, HyperSpace)> numerating = new List<(Vector2Int, HyperSpace)>();
                int minX = int.MaxValue, minY = int.MaxValue;
                foreach (var edge in edges)
                {
                    Vector2 res =(firstEdge.mainSquare.center - edge.mainSquare.center);
                    res.x /= firstEdge.mainSquare.size.x;
                    res.y /= firstEdge.mainSquare.size.y;
                    Vector2Int position = new Vector2Int((int)res.x, (int)res.y);
                    if (minX > position.x)
                        minX = position.x;
                    if (minY > position.y)
                        minY = position.y;
                    numerating.Add(new(position,edge));
                }
                Vector2Int additive = Vector2Int.zero;
                if (minX < 0)
                {
                    additive.x = -minX;
                }
                if (minY < 0)
                {
                    additive.y = -minY;
                }
                HyperSpace[,] matrix = new HyperSpace[additive.x+1, additive.y+1];
                foreach (var item in numerating)
                {
                    int x = item.Item1.x + additive.x;
                    int y = item.Item1.y + additive.y;
                    try
                    {
                        matrix[x, y] = item.Item2;
                    }
                    catch (Exception e)
                    {
                    }
                }

                for (int i = 0; i <= additive.x; i++)
                {
                    for (int j = 0; j <= additive.y; j++)
                    {
                        if(matrix[i,j]is null)
                            continue;
                        if (i + 1 != additive.x + 1 && j + 1 != additive.y + 1)
                        {
                            if (matrix[i + 1, j] is null && matrix[i, j + 1] is null)
                            {
                                var hSpace = matrix[i, j];
                                List<Vector2> points = new List<Vector2>();
                                points.Add(new Vector2(hSpace.mainSquare.max.x, hSpace.mainSquare.min.y));
                                points.Add(new Vector2(hSpace.mainSquare.min.x, hSpace.mainSquare.max.y));
                                points.Add(hSpace.mainSquare.max);
                                hSpace.cachedBMesh = CreateMeshFromPoints(points, hSpace.mainSquare.center,
                                    hSpace.size);

                            }
                        }
                        if (i - 1 != -1 && j + 1 != additive.y + 1)
                        {
                            if (matrix[i - 1, j] is null && matrix[i, j + 1] is null)
                            {
                                var hSpace = matrix[i, j];
                                List<Vector2> points = new List<Vector2>();
                                points.Add(hSpace.mainSquare.max);
                                points.Add(hSpace.mainSquare.min);
                                points.Add(new Vector2(hSpace.mainSquare.min.x, hSpace.mainSquare.max.y));
                                hSpace.cachedBMesh = CreateMeshFromPoints(points, hSpace.mainSquare.center,
                                    hSpace.size);

                            }
                        }
                        if (i + 1 != additive.x + 1 && j - 1 != -1)
                        {
                            if (matrix[i + 1, j] is null && matrix[i, j - 1] is null)
                            {
                                var hSpace = matrix[i, j];
                                List<Vector2> points = new List<Vector2>();
                                points.Add(hSpace.mainSquare.max);
                                points.Add(new Vector2(hSpace.mainSquare.max.x, hSpace.mainSquare.min.y));
                                points.Add(hSpace.mainSquare.min);
                                hSpace.cachedBMesh = CreateMeshFromPoints(points, hSpace.mainSquare.center,
                                    hSpace.size);

                            }
                        }
                        if (i - 1 !=  - 1 && j - 1 != -1)
                        {
                            if (matrix[i - 1, j] is null && matrix[i, j - 1] is null)
                            {
                                var hSpace = matrix[i, j];
                                List<Vector2> points = new List<Vector2>();
                                points.Add(new Vector2(hSpace.mainSquare.max.x, hSpace.mainSquare.min.y));
                                points.Add(hSpace.mainSquare.min);
                                points.Add(new Vector2(hSpace.mainSquare.min.x, hSpace.mainSquare.max.y));
                                hSpace.cachedBMesh = CreateMeshFromPoints(points, hSpace.mainSquare.center,
                                    hSpace.size);

                            }
                        }
                    }
                }
            }

            public BMesh GetBMesh(bool skipFirst = false)
            {
                if (isEmpty&&!skipFirst)
                {
                    return null;
                }

                if (cachedBMesh != null)
                    return cachedBMesh;

                CacheBMesh(skipFirst);
                return cachedBMesh;
            }

            private void CacheBMesh(bool skipFirst = false)
            {
                if (isCut && (!isEmpty || skipFirst))
                {
                    List<BMesh> bMeshes = new List<BMesh>();
                    for (int i = 0; i < splitCount; i++)
                    {
                        for (int j = 0; j < splitCount; j++)
                        {
                            var hspace = subSpaces[i, j];
                            if (hspace.isEmpty)
                            {
                                continue;
                            }

                            var mesh = hspace.GetBMesh();
                            if (mesh != null)
                                bMeshes.Add(mesh);
                        }
                    }

                    /*
                    foreach (var subSpace in subSpaces)
                    {
                        if (subSpace.isEmpty)
                            continue;
                        var mesh = subSpace.GetBMesh();
                        if (mesh != null)
                            bMeshes.Add(mesh);
                    }
                    */

                    cachedBMesh = bMeshes[0].Copy();
                    if (bMeshes.Count > 1)
                    {
                        for (int i = 1; i < bMeshes.Count; i++)
                        {
                            BMeshOperators.Merge(cachedBMesh, bMeshes[i]);
                        }
                    }
                    else
                    {
                        BMeshOperators.Merge(cachedBMesh, bMeshes[0]);
                    }
                }
                else
                {
                    cachedBMesh = isEmpty ? null : CreateQuadMesh(size, mainSquare.max, mainSquare.min);
                }
            }

            private void GenerateSubSpaces()
            {
                subSpaces = new HyperSpace[splitCount, splitCount];
                listedSubSpaces = new List<HyperSpace>();
                Vector2 boundsSize = mainSquare.size / splitCount;
                float currentX = 0f;
                float currentY = 0f;
                Vector2 startBottomLeft = (Vector2)mainSquare.min + boundsSize / 2;
                for (int i = 0; i < splitCount; i++)
                {
                    for (int j = 0; j < splitCount; j++)
                    {
                        subSpaces[i, j] = new HyperSpace(size,
                            startBottomLeft + Vector2.right * currentX + Vector2.up * currentY,
                            boundsSize,
                            maxDepth,
                            currentDepth + 1);
                        listedSubSpaces.Add(subSpaces[i, j]);
                        currentY += boundsSize.y;
                    }

                    currentY = 0f;
                    currentX += boundsSize.x;
                }
            }

            public void SplitBySquare(float minSquare, List<HyperSpace> result, int level)
            {
                if (!isCut || isEmpty)
                    return;
                if (currentDepth < level)
                {
                    foreach (var item in subSpaces)
                    {
                        item.SplitBySquare(minSquare, result, level);
                    }
                }
                else
                {
                    if (currentDepth == level)
                    {
                        var currentSquare = GetSquare();
                        if (currentSquare < minSquare && MathExtensions.GetRandomWithPercent(.5f))
                        {
                            result.Add(this);
                            isEmpty = true;
                        }
                    }
                }
            }
        }
        
        /*
        public class GridOfPoints
        {
            private List<Vector2> CurrentBounds;
            private List<(Vector3[],Bounds)> Holes;

            public GridOfPoints()
            {
                CurrentBounds = new List<Vector2>();
                Holes = new List<(Vector3[],Bounds)>();
                float stepLength = .1f;
                List<Vector2> steps = new List<Vector2>()
                {
                    Vector2.up, Vector2.right, Vector2.down, Vector2.left
                };
                Vector2 initialPos = Vector2.one * -.5f;
                for (int i = 0; i < 4; i++)
                {
                    var currentStep = steps[i];
                    currentStep *= stepLength;
                    for (int j = 0; j < (2f / stepLength); j++)
                    {
                        CurrentBounds.Add(initialPos);
                        initialPos += currentStep;
                    }
                }
            }

            public Geometry MakeHoleCircle(Vector2 position,float radius)
            {
                Bounds currentBounds = new Bounds
                {
                    center = position
                };
                var listPoints = new List<Vector2>();
                Vector2 offset = Vector2.one;
                offset *= radius;
                for (int i = 0; i <360 ; i++)
                {
                    listPoints.Add(MathExtensions.RotateNew(offset, i) + position);
                    currentBounds.Encapsulate(listPoints[^1]);
                }
                var parameters = new Triangulation2DParameters();
                parameters.Points = listPoints.Select(c=>(Vector3)c).ToArray();
                // foreach (var hole in Holes)
                // {
                //     if (hole.Item2.Intersects(currentBounds))
                //     {
                //         
                //     }
                // }
                Holes.Add((parameters.Points, currentBounds));
                parameters.Side = Side.Back;
                parameters.Delaunay = true;

                var triangulationAPI = new TriangulationAPI();
                var result = triangulationAPI.Triangulate2DRaw(parameters);
                return result;
                var mesh = triangulationAPI.Triangulate2D(parameters);
            }

            public Geometry GetBase()
            {
                var parameters = new Triangulation2DParameters();
                //parameters.Points = listedPoints.ToArray();
                parameters.Holes = Holes.Select(c => c.Item1).ToArray();
                parameters.Boundary = CurrentBounds.Select(c => (Vector3)c).ToArray();
                parameters.Side = Side.Back;
                parameters.Delaunay = true;

                var triangulationAPI = new TriangulationAPI();
                return triangulationAPI.Triangulate2DRaw(parameters);
            }
        }
        */
        
        public static Texture2D CopyTexture(Texture2D original)
        {
            Texture2D copyTexture = new Texture2D(original.width, original.height);
            copyTexture.SetPixels(original.GetPixels());
            copyTexture.Apply();
            return copyTexture;
        }
    }
}