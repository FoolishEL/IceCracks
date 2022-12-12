using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static BMesh;

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

        private BMesh cachedBMesh;

        public HyperSpace(Vector2 size, Vector2 boundsCenter, Vector2 boundsSize, int splitCount, int maxDepth,
            int currentDepth)
        {
            this.size = size;
            this.splitCount = splitCount;
            this.maxDepth = maxDepth;
            this.currentDepth = currentDepth;
            isEmpty = false;
            isCut = false;
            //mainSquare = new Rectangle(topLeftPoint, bottomRightPoint);
            mainSquare = new Bounds
            {
                center = boundsCenter,
                size = boundsSize
            };
        }

        public async Task CutOut(Bounds rectangle)
        {
            if (isEmpty)
                return;
            if (!mainSquare.Intersects(rectangle))
            {
                return;
            }

            if (currentDepth >= maxDepth ||
                (rectangle.Contains(mainSquare.max) && rectangle.Contains(mainSquare.min)))
            {
                cachedBMesh = null;
                isEmpty = true;
                return;
            }
            
            cachedBMesh = null;
            if (subSpaces is null)
                GenerateSubSpaces();
            isCut = true;
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
            {
                return null;
            }

            if (cachedBMesh != null)
                return cachedBMesh;
            
            CacheBMesh();
            return cachedBMesh;
        }

        private void CacheBMesh()
        {
            if (isCut && !isEmpty)
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
            Vector2 startTopLeft = (Vector2)mainSquare.min + boundsSize / 2;
            for (int i = 0; i < splitCount; i++)
            {
                for (int j = 0; j < splitCount; j++)
                {
                    subSpaces[i, j] = new HyperSpace(size,
                        startTopLeft + Vector2.right * currentX + Vector2.up * currentY,
                        boundsSize,
                        splitCount,
                        maxDepth,
                        currentDepth + 1);
                    listedSubSpaces.Add(subSpaces[i, j]);
                    currentY += boundsSize.y;
                }

                currentY = 0f;
                currentX += boundsSize.x;
            }
        }
    }
}