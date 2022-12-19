using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IceCracks.CracksGeneration.Extensions;
using UnityEngine;

namespace IceCracks.Utilities
{
    using CracksGeneration;
    public class HyperSpace
    {
        private Bounds mainSquare;
        private readonly Vector2 size;
        private readonly int splitCount;
        private readonly int maxDepth;
        private readonly int currentDepth;

        private Vector2Int startPosition, endPosition;
        //private Vector2Int currentId;

        private HyperSpace /*parent, */
            left, right, up, down;

        private bool isEmpty;
        private bool isTempted;
        private bool isCut;
        private bool isCurved;
        private List<HyperSpace> listedSubSpaces;

        private BMesh cachedBMesh;

        public HyperSpace(Vector2 size, Vector2 boundsCenter, Vector2 boundsSize, int maxDepth,
            int currentDepth, Vector2Int startPosition, Vector2Int endPosition)
        {
            this.size = size;
            splitCount = SplitMeshController.SplitAmounts.Count - 1 > currentDepth
                ? SplitMeshController.SplitAmounts[currentDepth]
                : SplitMeshController.SplitAmounts[0];
            this.maxDepth = maxDepth;
            this.currentDepth = currentDepth;
            isEmpty = false;
            isTempted = false;
            isCut = false;
            isCurved = false;
            mainSquare = new Bounds
            {
                center = boundsCenter,
                size = boundsSize
            };
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            if (maxDepth == currentDepth)
                if (endPosition - startPosition == Vector2Int.one)
                {
                    this.endPosition = startPosition;
                }

            if (currentDepth < maxDepth)
            {
                GenerateSubSpaces();
            }

            if (currentDepth == 0)
            {
                UniteSmallest();
            }
        }

        private void UniteSmallest()
        {
            if (currentDepth == maxDepth)
            {
                return;
            }

            if (left is not null)
            {
                for (int i = 0; i < splitCount; i++)
                {
                    listedSubSpaces[i * splitCount].left = left.listedSubSpaces[i * splitCount + splitCount - 1];
                }
            }

            if (right is not null)
            {
                for (int i = 0; i < splitCount; i++)
                {
                    listedSubSpaces[i * splitCount + splitCount - 1].right =
                        right.listedSubSpaces[i * splitCount];
                }
            }

            if (up is not null)
            {
                for (int i = 0; i < splitCount; i++)
                {
                    listedSubSpaces[(splitCount - 1) * splitCount + i].up =
                        up.listedSubSpaces[i];
                }
            }

            if (down is not null)
            {
                for (int i = 0; i < splitCount; i++)
                {
                    listedSubSpaces[i].down =
                        down.listedSubSpaces[splitCount * (splitCount - 1) + i];
                }
            }

            listedSubSpaces.ForEach(c => c.UniteSmallest());
        }

        public async Task CutOut(Bounds rectangleOrdinary, List<HyperSpace> onEdge)
        {
            if (!mainSquare.Intersects(rectangleOrdinary) || isEmpty)
            {
                return;
            }

            if (currentDepth >= maxDepth ||
                (rectangleOrdinary.Contains(mainSquare.max) && rectangleOrdinary.Contains(mainSquare.min)))
            {
                if (currentDepth >= maxDepth)
                {
                    if (Vector2.Distance(mainSquare.center, rectangleOrdinary.center) >
                        rectangleOrdinary.size.magnitude * .4f)
                    {
                        onEdge.Add(this);
                        return;
                    }
                }

                cachedBMesh = null;
                isEmpty = true;
                return;
            }

            cachedBMesh = null;
            // if (listedSubSpaces is null || listedSubSpaces.Count == 0)
            //     GenerateSubSpaces();
            isCut = true;
            await Task.WhenAll(listedSubSpaces.Select(c => c.CutOut(rectangleOrdinary, onEdge)));

            if (listedSubSpaces.Count(c => c.isEmpty) == splitCount * splitCount)
            {
                isEmpty = true;
                cachedBMesh = null;
            }
        }

        public async Task GetAll(Bounds bounds, List<HyperSpace> spaces)
        {
            // bool GetStatusConnected(HyperSpace space)
            // {
            //     return space == null || space.isEmpty && !space.mainSquare.Intersects(bounds);
            // }
            if (!mainSquare.Intersects(bounds) || isEmpty)
            {
                return;
            }

            if (currentDepth >= maxDepth ||
                (bounds.Contains(mainSquare.max) && bounds.Contains(mainSquare.min)))
            {
                spaces.Add(this);
                /*
                if (GetStatusConnected(left) && GetStatusConnected(up))
                {
                    Debug.LogError("left and up");
                    meshes.Add(CreateMeshFromPoints(
                        new List<Vector2>()
                            { mainSquare.min, mainSquare.max, new Vector2(mainSquare.max.x, mainSquare.min.y) },
                        mainSquare.center, size));
                    cachedBMesh = null;
                    isEmpty = true;
                    return;
                }
                if (GetStatusConnected(left) && GetStatusConnected(down))
                {
                    Debug.LogError("left and down");
                    meshes.Add(CreateMeshFromPoints(
                        new List<Vector2>()
                            { new Vector2(mainSquare.min.x, mainSquare.max.y), mainSquare.max, new Vector2(mainSquare.max.x, mainSquare.min.y) },
                        mainSquare.center, size));
                    cachedBMesh = null;
                    isEmpty = true;
                    return;
                }
                if (GetStatusConnected(right) && GetStatusConnected(up))
                {
                    Debug.LogError("right and up");
                    meshes.Add(CreateMeshFromPoints(
                        new List<Vector2>()
                            { mainSquare.min, mainSquare.max, new Vector2(mainSquare.max.x, mainSquare.min.y) },
                        mainSquare.center, size));
                    cachedBMesh = null;
                    isEmpty = true;
                    return;
                }
                if (GetStatusConnected(right) && GetStatusConnected(down))
                {
                    Debug.LogError("right and down");
                    meshes.Add(CreateMeshFromPoints(
                        new List<Vector2>()
                            { mainSquare.min, mainSquare.max, new Vector2(mainSquare.max.x, mainSquare.min.y) },
                        mainSquare.center, size));
                    cachedBMesh = null;
                    isEmpty = true;
                    return;
                }
                */
                isTempted = true;
                return;
            }

            cachedBMesh = null;
            if (listedSubSpaces is null || listedSubSpaces.Count == 0)
                GenerateSubSpaces();
            isCut = true;
            await Task.WhenAll(listedSubSpaces.Select(c => c.GetAll(bounds, spaces)));
            if (listedSubSpaces.Count(c => c.isEmpty || c.isTempted) == splitCount * splitCount)
            {
                isEmpty = true;
                cachedBMesh = null;
            }
        }

        public static BMesh GetMeshFromArea(List<HyperSpace> spaces, Bounds bounds)
        {
            bool GetStatusConnected(HyperSpace space)
            {
                return space == null || !space.isTempted /*|| !space.mainSquare.Intersects(bounds)*/;
            }

            List<BMesh> meshes = new List<BMesh>();
            foreach (var space in spaces)
            {
                if (space.isCurved)
                {
                    meshes.Add(space.cachedBMesh);
                    continue;
                }
                Direction dir = Direction.NONE;
                if (GetStatusConnected(space.left))
                    dir |= Direction.Left;
                if (GetStatusConnected(space.right))
                    dir |= Direction.Right;
                if (GetStatusConnected(space.up))
                    dir |= Direction.Up;
                if (GetStatusConnected(space.down))
                    dir |= Direction.Down;
                meshes.Add(space.GetBMeshFromDirection(dir));
            }

            BMesh first = meshes[0];
            for (int i = 1; i < meshes.Count; i++)
            {
                BMeshOperators.Merge(first, meshes[i]);
            }

            foreach (var space in spaces)
            {
                space.isTempted = false;
                space.isEmpty = true;
                space.cachedBMesh = null;
            }

            return first;
        }

        public static void AdjustBorders(List<HyperSpace> edges)
        {
            bool GetStatusConnected(HyperSpace space) => space is null || space.isEmpty;

            foreach (var space in edges)
            {
                if (space.isCurved)
                {
                    Debug.LogError("possible custom handle");
                    continue;
                }

                Direction dir = Direction.NONE;
                if (GetStatusConnected(space.left))
                    dir |= Direction.Left;
                if (GetStatusConnected(space.right))
                    dir |= Direction.Right;
                if (GetStatusConnected(space.up))
                    dir |= Direction.Up;
                if (GetStatusConnected(space.down))
                    dir |= Direction.Down;
                space.cachedBMesh = space.GetBMeshFromDirection(dir);
            }
        }

        public BMesh GetBMesh(bool skipFirst = false)
        {
            if (isEmpty && !skipFirst)
                return null;

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
                foreach (var hSpace in listedSubSpaces)
                {
                    if (hSpace.isEmpty)
                    {
                        continue;
                    }

                    var mesh = hSpace.GetBMesh();
                    if (mesh != null)
                        bMeshes.Add(mesh);
                }

                cachedBMesh = bMeshes[0].Copy();
                if (bMeshes.Count > 1)
                {
                    for (int i = 1; i < bMeshes.Count; i++)
                        BMeshOperators.Merge(cachedBMesh, bMeshes[i]);
                }
                else
                    BMeshOperators.Merge(cachedBMesh, bMeshes[0]);
            }
            else
            {
                cachedBMesh = isEmpty ? null : BMeshUtilities.CreateQuadMesh(size, mainSquare.max, mainSquare.min);
            }
        }

        private void GenerateSubSpaces()
        {
            listedSubSpaces = new List<HyperSpace>();
            Vector2 boundsSize = mainSquare.size / splitCount;
            float currentX = 0f;
            float currentY = 0f;
            int currentStep = (endPosition.x - startPosition.x + 1) / splitCount;
            Vector2 startBottomLeft = (Vector2)mainSquare.min + boundsSize / 2;

            for (int i = 0; i < splitCount; i++)
            {
                for (int j = 0; j < splitCount; j++)
                {
                    Vector2Int startPos = startPosition + Vector2Int.up * i * currentStep
                                                        + Vector2Int.right * j * currentStep;
                    listedSubSpaces.Add(
                        new HyperSpace(size,
                            startBottomLeft + Vector2.right * currentX + Vector2.up * currentY,
                            boundsSize,
                            maxDepth,
                            currentDepth + 1, startPos, startPos + Vector2Int.one * currentStep)
                    );
                    //listedSubSpaces[^1].parent = this;
                    //listedSubSpaces[^1].currentId = new Vector2Int(i, j);
                    currentY += boundsSize.y;
                }

                currentY = 0f;
                currentX += boundsSize.x;
            }

            int GetId(int i, int j)
            {
                return i * splitCount + j;
            }

            for (int i = 0; i < splitCount - 1; i++)
            {
                for (int j = 0; j < splitCount - 1; j++)
                {
                    listedSubSpaces[GetId(i, j)].right = listedSubSpaces[GetId(i, j + 1)];
                    listedSubSpaces[GetId(i, j + 1)].left = listedSubSpaces[GetId(i, j)];
                    listedSubSpaces[GetId(i, j)].up = listedSubSpaces[GetId(i + 1, j)];
                    listedSubSpaces[GetId(i + 1, j)].down = listedSubSpaces[GetId(i, j)];
                }

                listedSubSpaces[GetId(i, splitCount - 1)].up =
                    listedSubSpaces[GetId(i + 1, splitCount - 1)];
                listedSubSpaces[GetId(i + 1, splitCount - 1)].down =
                    listedSubSpaces[GetId(i, splitCount - 1)];

                listedSubSpaces[GetId(splitCount - 1, i)].right =
                    listedSubSpaces[GetId(splitCount - 1, i + 1)];
                listedSubSpaces[GetId(splitCount - 1, i + 1)].left =
                    listedSubSpaces[GetId(splitCount - 1, i)];
            }

            listedSubSpaces[^1].left = listedSubSpaces[^2];
            listedSubSpaces[^2].right = listedSubSpaces[^1];
            listedSubSpaces[^1].down = listedSubSpaces[GetId(splitCount - 2, splitCount - 1)];
            listedSubSpaces[GetId(splitCount - 2, splitCount - 1)].up = listedSubSpaces[^1];
        }
        
        [Flags]
        public enum Direction
        {
            NONE =0,
            Up = 1,
            Down = 2,
            Right = 4,
            Left = 8,

            //2
            UpRight = Up | Right,
            UpLeft = Up | Left,
            DownRight = Down | Right,
            DownLeft = Down | Left,
            UpDown = Down | Up,
            RightLeft = Right | Left,

            //3
            UpRightLeft = UpRight | Left,
            UpDownLeft = UpDown | Left,
            RightLeftDown = RightLeft | Down,
            UpDownRight = UpDown | Right,

            //4
            ALL = UpRightLeft | Down,
        }

        public BMesh GetBMeshFromDirection(Direction direction)
        {
            float depth = -.2f;
            float offset = .3f;
            if (direction.HasFlag(Direction.ALL))
            {
                return BMeshUtilities.CreateVShape(new List<Vector2>()
                {
                    mainSquare.OnTop(.2f),
                    mainSquare.OnTop(.8f),
                    mainSquare.OnRight(.2f),
                    mainSquare.OnRight(.8f),
                    mainSquare.OnBottom(.8f),
                    mainSquare.OnBottom(.2f),
                    mainSquare.OnLeft(.8f),
                    mainSquare.OnLeft(.2f)
                }, size, out _, null, offset);
            }
            if (direction.HasFlag(Direction.UpRightLeft))
            {
                return BMeshUtilities.CreateVShape(new List<Vector2>()
                {
                    mainSquare.BottomLeft(),
                    mainSquare.OnRight(.7f),
                    mainSquare.OnRight(.3f),
                    mainSquare.TopLeft(),
                }, size, out _, new List<int>(){3}, offset);
            }
            if (direction.HasFlag(Direction.UpDownLeft))
            {
                return BMeshUtilities.CreateVShape(new List<Vector2>()
                {
                    mainSquare.TopLeft(),
                    mainSquare.OnBottom(.3f),
                    mainSquare.OnBottom(.7f),
                    mainSquare.TopRight(),
                }, size, out _, new List<int>(){3}, offset);
            }
            if (direction.HasFlag(Direction.RightLeftDown))
            {
                return BMeshUtilities.CreateVShape(new List<Vector2>()
                {
                    mainSquare.TopRight(),
                    mainSquare.OnLeft(.3f),
                    mainSquare.OnLeft(.7f),
                    mainSquare.BottomRight(),
                }, size, out _, new List<int>(){3}, offset);
            }
            if (direction.HasFlag(Direction.UpDownRight))
            {
                return BMeshUtilities.CreateVShape(new List<Vector2>()
                {
                    mainSquare.BottomRight(),
                    mainSquare.OnTop(.7f),
                    mainSquare.OnTop(.3f),
                    mainSquare.BottomLeft(),
                }, size, out _, new List<int>(){3}, offset);
            }
            if (direction.HasFlag(Direction.UpRight))
            {
                var mesh = BMeshUtilities.CreateMeshFromPoints(new List<Vector2>()
                {
                    mainSquare.BottomLeft(),
                    mainSquare.BottomRight(),
                    mainSquare.TopLeft(),
                }, size);
                BMeshOperators.Merge(mesh,
                    BMeshUtilities.CreateDownSide(mainSquare.BottomRight(), mainSquare.TopLeft(),
                        mainSquare.BottomLeft(), mainSquare.BottomLeft(),depth, offset, size));
                return mesh;
            }
            if (direction.HasFlag(Direction.UpLeft))
            {
                var mesh = BMeshUtilities.CreateMeshFromPoints(new List<Vector2>()
                {
                    mainSquare.BottomLeft(),
                    mainSquare.TopRight(),
                    mainSquare.TopLeft(),
                }, size);
                BMeshOperators.Merge(mesh,
                    BMeshUtilities.CreateDownSide(mainSquare.BottomLeft(), mainSquare.TopRight(),
                        mainSquare.TopLeft(),mainSquare.TopLeft(), depth, offset, size));
                return mesh;
            }
            if (direction.HasFlag(Direction.DownRight))
            {
                var mesh = BMeshUtilities.CreateMeshFromPoints(new List<Vector2>()
                {
                    mainSquare.BottomLeft(),
                    mainSquare.BottomRight(),
                    mainSquare.TopRight(),
                }, size);
                BMeshOperators.Merge(mesh,
                    BMeshUtilities.CreateDownSide(mainSquare.TopRight(), mainSquare.BottomLeft(),
                        mainSquare.BottomRight(),mainSquare.BottomRight(), depth, offset, size));
                return mesh;
            }
            if (direction.HasFlag(Direction.DownLeft))
            {
                var mesh = BMeshUtilities.CreateMeshFromPoints(new List<Vector2>()
                {
                    mainSquare.TopLeft(),
                    mainSquare.BottomRight(),
                    mainSquare.TopRight(),
                }, size);
                BMeshOperators.Merge(mesh,
                    BMeshUtilities.CreateDownSide(mainSquare.TopLeft(), mainSquare.BottomRight(),
                        mainSquare.TopRight(),mainSquare.TopRight(), depth, offset, size));
                return mesh;
            }
            if (direction.HasFlag(Direction.Up))
            {
                var mesh = BMeshUtilities.CreateQuadMesh(size, mainSquare.max, mainSquare.min);
                BMeshOperators.Merge(mesh,
                    BMeshUtilities.CreateDownSide( mainSquare.BottomRight(),mainSquare.TopRight(),
                         mainSquare.BottomLeft(),mainSquare.TopLeft(), depth, offset, size));
                return mesh;
            }
            if (direction.HasFlag(Direction.Left))
            {
                var mesh = BMeshUtilities.CreateQuadMesh(size, mainSquare.max, mainSquare.min);
                BMeshOperators.Merge(mesh,
                    BMeshUtilities.CreateDownSide( mainSquare.BottomLeft(),mainSquare.BottomRight(),
                        mainSquare.TopLeft(),mainSquare.TopRight(), depth, offset, size));
                return mesh;
            }
            if (direction.HasFlag(Direction.Down))
            {
                var mesh = BMeshUtilities.CreateQuadMesh(size, mainSquare.max, mainSquare.min);
                BMeshOperators.Merge(mesh,
                    BMeshUtilities.CreateDownSide( mainSquare.TopLeft(),mainSquare.BottomLeft(),
                        mainSquare.TopRight(),mainSquare.BottomRight(), depth, offset, size));
                return mesh;
            }
            if (direction.HasFlag(Direction.Right))
            {
                var mesh = BMeshUtilities.CreateQuadMesh(size, mainSquare.max, mainSquare.min);
                BMeshOperators.Merge(mesh,
                    BMeshUtilities.CreateDownSide( mainSquare.TopRight(),mainSquare.TopLeft(),
                        mainSquare.BottomRight(),mainSquare.BottomLeft(), depth, offset, size));
                return mesh;
            }

            return BMeshUtilities.CreateQuadMesh(size, mainSquare.max, mainSquare.min);
        }
    }
}