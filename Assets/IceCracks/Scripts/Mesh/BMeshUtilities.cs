using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static BMesh;

public static class BMeshUtilities
{
    public static BMesh CreateQuadMesh(Vector2 size, Vector2 topLeftPoint, Vector2 bottomRightPoint)
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

    private static Vertex AddVertexToMesh(float x, float y, BMesh bMesh, Vector2 size)
    {
        float initialX = (x + 1f) / 2f;
        float initialY = (y + 1f) / 2f;
        x *= (size.x / 2f);
        y *= (size.y / 2f);
        var vert = bMesh.AddVertex(x, 0, y);
        vert.attributes["uv"] = new BMesh.FloatAttributeValue(initialX, initialY);
        return vert;
    }

    public class Rectangle
    {
        public Vector2 topLeftPoint { get; private set; }
        public Vector2 bottomRightPoint { get; private set; }
        
        public Vector2 topRightPoint { get; private set; }
        public Vector2 bottomLeftPoint { get; private set; }

        public Vector2 centerPoint { get; private set; }
        private Vector2 deltaSize;

        public Rectangle(Vector2 topLeftPoint, Vector2 bottomRightPoint)
        {
            if (topLeftPoint.x > bottomRightPoint.x)
                (bottomRightPoint.x, topLeftPoint.x) = (topLeftPoint.x, bottomRightPoint.x);
            if (bottomRightPoint.y > topLeftPoint.y)
                (topLeftPoint.y, bottomRightPoint.y) = (bottomRightPoint.y, topLeftPoint.y);

            this.topLeftPoint = topLeftPoint;
            this.bottomRightPoint = bottomRightPoint;
            deltaSize = new Vector2(Mathf.Abs(topLeftPoint.x - centerPoint.x),
                Mathf.Abs(topLeftPoint.x - centerPoint.x));
            centerPoint = (topLeftPoint + bottomRightPoint) / 2;
            topRightPoint = new Vector2(bottomRightPoint.x, topLeftPoint.y);
            bottomLeftPoint = new Vector2(topLeftPoint.x, bottomRightPoint.y);
        }

        public BMesh ToBMesh(Vector2 size)
        {
            return CreateQuadMesh(size, topLeftPoint, bottomRightPoint);
        }

        public bool IsPointInside(Vector2 point)
        {
            return point.x <= bottomRightPoint.x && point.x >= topLeftPoint.x && 
                   point.y >= bottomRightPoint.y && point.y <= topLeftPoint.y;
        }

        public bool IsIntersectedWith(Rectangle another)
        {
            if (Vector2.Distance(centerPoint, another.centerPoint) > Vector2.Distance(centerPoint, topLeftPoint) +
                Vector2.Distance(another.centerPoint, another.topLeftPoint))
                return false;
            bool isCoreInside = IsPointInside(another.topLeftPoint) || IsPointInside(another.bottomRightPoint) ||
                                IsPointInside(another.topRightPoint) || IsPointInside(another.bottomLeftPoint) ||

                                another.IsPointInside(topLeftPoint) || another.IsPointInside(bottomRightPoint) ||
                                another.IsPointInside(topRightPoint) || another.IsPointInside(bottomRightPoint);
            return isCoreInside;
        }

        public bool IsInsideIn(Rectangle other)
        {
            return other.IsPointInside(topLeftPoint)
                   && other.IsPointInside(bottomRightPoint);
        }
    }

    public class HyperSpace
    {
        private Rectangle mainSquare;
        private Vector2 size;
        private int splitCount;
        private int maxDepth;
        private int currentDepth;

        private bool isEmpty;
        private bool isCutted;
        private HyperSpace[,] subSpaces;
        private List<HyperSpace> listedSubSpaces;

        private BMesh cachedBMesh;

        private Vector2 topLeft => mainSquare.topLeftPoint;
        private Vector2 bottomRight => mainSquare.bottomRightPoint;

        public HyperSpace(Vector2 size, Vector2 topLeftPoint, Vector2 bottomRightPoint, int splitCount, int maxDepth,
            int currentDepth)
        {
            this.size = size;
            this.splitCount = splitCount;
            this.maxDepth = maxDepth;
            this.currentDepth = currentDepth;
            isEmpty = false;
            isCutted = false;
            mainSquare = new Rectangle(topLeftPoint, bottomRightPoint);
        }

        public async Task CutOut(Rectangle rectangle)
        {
            if (isEmpty)
                return;
            if (!mainSquare.IsIntersectedWith(rectangle))
            {
                return;
            }
            if (currentDepth >= maxDepth || mainSquare.IsInsideIn(rectangle))
            {
                cachedBMesh = null;
                isEmpty = true;
                return;
            }
            
            cachedBMesh = null;
            if (subSpaces is null)
                GenerateSubSpaces();
            isCutted = true;
            int emptinessCount = 0;
            //await Task.WhenAll(listedSubSpaces.Select(c => c.CutOut(rectangle)));

            #region unuptimized
            
            for (int i = 0; i < splitCount; i++)
            {
                for (int j = 0; j < splitCount; j++)
                {
                    var hspace = subSpaces[i, j];
                    await hspace.CutOut(rectangle);
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
                isEmpty = true;
                cachedBMesh = null;
            }
        }

        public BMesh GetBMesh()
        {
            if (isEmpty)
                return null;
            if (cachedBMesh != null)
                return cachedBMesh;
            
            CacheBMesh();
            return cachedBMesh;
        }

        private void CacheBMesh()
        {
            if (isCutted && !isEmpty)
            {
                List<BMesh> bMeshes = new List<BMesh>();
                for (int i = 0; i < splitCount; i++)
                {
                    for (int j = 0; j < splitCount; j++)
                    {
                        var hspace = subSpaces[i, j];
                        if (hspace.isEmpty)
                            continue;

                        var mesh = hspace.GetBMesh();
                        if (mesh != null)
                            bMeshes.Add(mesh);
                    }
                }
                // foreach (var subSpace in subSpaces)
                // {
                //     if (subSpace.isEmpty)
                //         continue;
                //     var mesh = subSpace.GetBMesh();
                //     if (mesh != null)
                //         bMeshes.Add(mesh);
                // }

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
                cachedBMesh = isEmpty ? null : mainSquare.ToBMesh(size);
            }
        }

        private void GenerateSubSpaces()
        {
            subSpaces = new HyperSpace[splitCount, splitCount];
            listedSubSpaces = new List<HyperSpace>();
            float xOffset = (bottomRight.x - topLeft.x) / splitCount;
            float yOffset = (bottomRight.y - topLeft.y) / splitCount;
            float currentX = 0f;
            float currentY = 0f;
            Vector2 startTopLeft = new Vector2(topLeft.x, topLeft.y);
            Vector2 startBottomRight = new Vector2(topLeft.x + xOffset, topLeft.y + yOffset);
            for (int i = 0; i < splitCount; i++)
            {
                for (int j = 0; j < splitCount; j++)
                {
                    subSpaces[i, j] = new HyperSpace(size,
                        startTopLeft + Vector2.right * currentX + Vector2.up * currentY,
                        startBottomRight + Vector2.right * currentX + Vector2.up * currentY, splitCount, maxDepth,
                        currentDepth + 1);
                    listedSubSpaces.Add(subSpaces[i, j]);
                    currentY += yOffset;
                }

                currentY = 0f;
                currentX += xOffset;
            }
        }
    }
}