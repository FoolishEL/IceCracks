using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IceCracks.Math;
using UnityEngine;
using static BMesh;

namespace IceCracks.Utilities
{
    using CracksGeneration;

    public static class BMeshUtilities
    {
        public static BMesh CreateQuadMesh(Vector2 size, Vector2 topRightPoint, Vector2 bottomLeftPoint)
        {
            BMesh mesh = new BMesh();
            var uv = mesh.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);

            List<Vertex> vertices = new List<Vertex>();
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
            List<Vertex> vertices = new List<Vertex>();
            vertices.Add(AddVertexToMesh(center.x, center.y, mesh, size));
            vertices.Add(AddVertexToMesh(points[0].x, points[0].y, mesh, size));
            vertices.Add(AddVertexToMesh(points[^1].x, points[^1].y, mesh, size));
            mesh.AddFace(vertices.ToArray());
            
            for (int i = 0; i < points.Count - 1; i++)
            {
                BMesh meshTemp = new BMesh();
                meshTemp.AddVertexAttribute("uv", BMesh.AttributeBaseType.Float, 2);
                List<Vertex> verticesTemp = new List<Vertex>();
                verticesTemp.Add(AddVertexToMesh(center.x, center.y, meshTemp, size));
                verticesTemp.Add(AddVertexToMesh(points[i+1].x, points[i+1].y, meshTemp, size));
                verticesTemp.Add(AddVertexToMesh(points[i].x, points[i].y, meshTemp, size));
                meshTemp.AddFace(verticesTemp.ToArray());
                BMeshOperators.Merge(mesh, meshTemp);
            }
            return mesh;
        }

        private static Vertex AddVertexToMesh(float x, float y, BMesh bMesh, Vector2 size)
        {
            float initialX = (x + 1f) / 2f;
            float initialY = (1f - y) / 2f;
            x *= (size.x / 2f);
            y *= (size.y / 2f);
            var vert = bMesh.AddVertex(x, 0, y);
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
            private List<Bounds> emptyBy;
            private List<Bounds> cutBy;

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
                emptyBy = new List<Bounds>();
                cutBy = new List<Bounds>();
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

            public async Task CutOut(Bounds rectangle,List<HyperSpace>onEdge)
            {
                if (!mainSquare.Intersects(rectangle))
                {
                    return;
                }

                if (isEmpty)
                    return;

                if (currentDepth >= maxDepth ||
                    (rectangle.Contains(mainSquare.max) && rectangle.Contains(mainSquare.min)))
                {
                    if (currentDepth >= maxDepth)
                    {
                        if (Vector2.Distance(mainSquare.center, rectangle.center) > rectangle.size.magnitude*.4f)
                        {
                            onEdge.Add(this);
                            return;
                        }
                    }
                    //empty.Add(this);
                    emptyBy.Add(rectangle);
                    cachedBMesh = null;
                    isEmpty = true;
                    return;
                }

                cachedBMesh = null;
                if (subSpaces is null)
                    GenerateSubSpaces();
                isCut = true;
                cachedSquare = -1f;
                cutBy.Add(rectangle);
                int emptinessCount = 0;
                //await Task.WhenAll(listedSubSpaces.Select(c => c.CutOut(rectangle)));

                #region unuptimized

                for (int i = 0; i < splitCount; i++)
                {
                    for (int j = 0; j < splitCount; j++)
                    {
                        var hspace = subSpaces[i, j];
                        await hspace.CutOut(rectangle, onEdge);
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
                    if (cutBy.Count > 1)
                    {
                        emptyBy.AddRange(cutBy);
                    }
                    else
                    {
                        emptyBy.Add(rectangle);
                    }

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

        public static Texture2D CopyTexture(Texture2D original)
        {
            Texture2D copyTexture = new Texture2D(original.width, original.height);
            copyTexture.SetPixels(original.GetPixels());
            copyTexture.Apply();
            return copyTexture;
        }
    }
}