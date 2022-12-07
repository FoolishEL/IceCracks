using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IceCracks.CracksGeneration.Models
{
    using Math;
    using Extensions;

    public class CrackCore : CrackArea
    {
        public readonly Vector2Int position;
        public readonly float radius;

        private readonly List<CrackLineGroup> connectedLines;
        public IReadOnlyList<CrackLineGroup> ConnectedLines => connectedLines;
        private readonly List<(Vector2Int, Vector2Int)> crackedLines;
        private readonly List<Vector2Int> exitCrackPositions;

        public CrackCore(Vector2Int position, float radius, out IReadOnlyList<CrackLineGroup> generated,
            float force = 10f)
        {
            hasStateChangedSinceLastPointsGet = true;
            this.position = position;
            this.radius = radius;
            connectedLines = new List<CrackLineGroup>();
            exitCrackPositions = new List<Vector2Int>();
            crackedLines = new List<(Vector2Int, Vector2Int)>();
            GeneratePoints();
            if (force > CrackExtensions.TOKEN_DEFAULT_CORE_FORCE_VALUE +
                CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE)
            {
                force -= CrackExtensions.TOKEN_DEFAULT_CORE_FORCE_VALUE;
                ProlongCrack(force);
            }

            generated = connectedLines;
        }

        public override bool IsIntersect(Vector2 point)
        {
            return Vector2.Distance(point, position) <= radius;
        }

        public override bool IsIntersectLine(Vector2 sPoint, Vector2 ePoint)
        {
            if (IsIntersect(sPoint) || IsIntersect(ePoint))
                return true;
            Vector2 center = (sPoint + ePoint) / 2;
            float distanceFromCenter = Vector2.Distance(center, position);
            float distanceFromStart = Vector2.Distance(sPoint, position);
            float distanceFromEnd = Vector2.Distance(ePoint, position);
            if (distanceFromCenter < distanceFromEnd && distanceFromCenter < distanceFromStart)
            {
                for (float step = .0f; step <= 1f; step += 0.02f)
                {
                    if (IsIntersect(Vector2.Lerp(sPoint, ePoint, step)))
                        return true;
                }
            }

            return false;
        }

        public override bool IsIntersectedWith(CrackArea area) => area switch
        {
            CrackCore core => this.IsIntersected(core),
            CrackLine line => this.IsIntersected(line),
            CrackLineGroup group => group.Lines.Any(this.IsIntersected),
            _ => throw new NotImplementedException()
        };

        public override IReadOnlyCollection<(Vector2Int, Vector2Int)> GetPoints()
        {
            if (crackedLines is null || crackedLines.Count == 0)
                GeneratePoints();
            hasStateChangedSinceLastPointsGet = false;
            return crackedLines;
        }

        private void GeneratePoints()
        {
            //TODO: feature for later
            CrackExtensions.GenerateValuesWithSum(360,
                /*((int)(radius / CrackExtensions.TOKEN_DEFAULT_CORE_SPLIT_RADIUS) *
                 CrackExtensions.TOKEN_DEFAULT_MIN_SECTORS_COUNT
                 )*/
                Random.Range(CrackExtensions.TOKEN_DEFAULT_MIN_SECTORS_COUNT,
                    CrackExtensions.TOKEN_DEFAULT_MAX_SECTORS_COUNT)
                , out var result);
            
            foreach (var p in result)
            {
                CrackExtensions.SphericalToCartesian(radius, p, out var vector);
                vector += position;
                exitCrackPositions.Add(new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y)));
            }

            crackedLines.AddRange(CrackExtensions.SplitCoreLines(exitCrackPositions, position));
        }

        /// <summary>
        /// Prolongs cracks with given force.
        /// NOTE: if using internal don't add generated new cracks to crackedLines;
        /// </summary>
        public IEnumerable<CrackLineGroup> ProlongCrack(float force)
        {
            var generatedCracks = new List<CrackLineGroup>();
            if (connectedLines.Count == 0)
                generatedCracks.AddRange(RawProlongCreate(force));
            else
            {
                float forceCreate = 0f;
                float forceAdditive = 0f;
                var spareOuterPoints = GetSparePositions().ToList();
                if (spareOuterPoints.Count == 0)
                {
                    forceAdditive = force;
                }
                else
                {
                    //spareOuterPoints.Count > connectedLines.Count
                    //TODO: move .7 to constants!
                    if ((float)spareOuterPoints.Count / exitCrackPositions.Count > .7f ||
                        MathExtensions.GetRandomWithPercent(.8f))
                    {
                        int canCreate = Mathf.FloorToInt(force) % (int)CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE;
                        if (canCreate >= 3)
                        {
                            forceCreate = force * 2 / 3;
                            forceAdditive = force - forceCreate;
                        }
                        else
                        {
                            forceCreate = force;
                            forceAdditive = 0f;
                        }
                    }
                    else
                    {
                        forceAdditive = force;
                    }
                }

                //TODO: upd!
                if (forceCreate > CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE)
                    generatedCracks.AddRange(RawProlongCreate(forceCreate));
                if (forceAdditive > CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE)
                    ProlongExistedLines(forceAdditive);
            }

            connectedLines.AddRange(generatedCracks);
            return generatedCracks;
        }

        private void ProlongExistedLines(float force)
        {
            if (connectedLines.Count == 0)
            {
                Debug.LogError("!!!!!");
            }

            var connectedLinesCopy = new List<CrackLineGroup>();
            connectedLinesCopy.AddRange(connectedLines);
            while (force > CrackExtensions.TOKEN_MAXIMUM_LINE_FORCE_VALUE +
                   CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE && connectedLinesCopy.Count > 2)
            {
                var additiveForce = Random.Range(CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE,
                    CrackExtensions.TOKEN_MAXIMUM_LINE_FORCE_VALUE);
                var selectedLine = connectedLinesCopy[Random.Range(0, connectedLinesCopy.Count)];
                selectedLine.ProlongCrack(additiveForce);
                force -= additiveForce;
                connectedLinesCopy.Remove(selectedLine);
            }
#if UNITY_EDITOR
            if (connectedLinesCopy.Count == 0)
            {
                Debug.LogError("[DEBUG]: Pay attention to this!");
            }
#endif

            connectedLinesCopy.First().ProlongCrack(force);
        }

        private IEnumerable<CrackLineGroup> RawProlongCreate(float force)
        {
            var newCracks = new List<CrackLineGroup>();
            var selectedPositions = new List<Vector2Int>();
            while (force > CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE)
            {
                float currentForce =
                    Random.Range(CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE,
                        Mathf.Min(CrackExtensions.TOKEN_MAXIMUM_LINE_FORCE_VALUE, force));
                force -= currentForce;
                if (force < CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE)
                {
                    currentForce += force;
                }

                var generatedPosition = GetStartPosition(selectedPositions);
                if (generatedPosition.HasValue)
                {
                    var newPosition = generatedPosition.Value;
                    newCracks.Add(
                        CrackLineGroup.CreateGroupByForceDirectionStartPosition(currentForce, newPosition - position,
                            newPosition));
                    selectedPositions.Add(newPosition);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("No spare position founded! Might be an error, but nothing bad happened tho.");
                }
#endif
            }

            NotifyInternalStateChanged();
            return newCracks;
        }

        private IEnumerable<Vector2Int> GetSparePositions()
        {
            var sparePositions = new List<Vector2Int>();
            sparePositions.AddRange(exitCrackPositions);
            foreach (var connectedLine in connectedLines)
            {
                //TODO: Random generation values hardcoded!
                if (connectedLine.GetLinesAmount() != 0)
                {
                    sparePositions.Remove(connectedLine.startPoint);
                }
            }

            return sparePositions;
        }

        private Vector2Int? GetStartPosition(IReadOnlyList<Vector2Int> selectedPositions)
        {
            List<Vector2Int> sparePositions = new List<Vector2Int>();
            sparePositions.AddRange(GetSparePositions());
            if (sparePositions.Count == 0)
                return null;

            if (selectedPositions is null || selectedPositions.Count == 0)
            {
                return sparePositions[Random.Range(0, sparePositions.Count)];
            }

            foreach (var sPos in selectedPositions)
            {
                sparePositions.Remove(sPos);
            }

            int startSparePosCount = sparePositions.Count;
            sparePositions = sparePositions
                .OrderByDescending(c => selectedPositions.Sum(k => Vector2Int.Distance(c, k))).ToList();
            while (sparePositions.Count > startSparePosCount * .5f && sparePositions.Count > 2)
            {
                sparePositions.RemoveAt(sparePositions.Count - 1);
            }

            if (sparePositions.Count == 0)
                return null;
            return sparePositions[Random.Range(0, sparePositions.Count)];
        }
    }
}