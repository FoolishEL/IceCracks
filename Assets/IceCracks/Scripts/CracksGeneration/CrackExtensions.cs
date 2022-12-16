using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IceCracks.CracksGeneration.Extensions
{
    using Models;
    
    public static class CrackExtensions
    {
        public static bool IsIntersected(this CrackCore core, CrackCore other)
        {
            return Vector2.Distance(core.position, other.position) < core.radius + other.radius;
        }

        public static bool IsIntersected(this CrackCore core, CrackLine line)
        {
            return core.IsIntersectLine(line.startPoint, line.endPoint);
        }

        public static bool IsIntersected(this CrackLine line, CrackLine other)
        {
            return line.IsIntersectLine(other.startPoint, other.endPoint);
        }

        public static void GenerateValuesWithSum(int sum, int count, out IReadOnlyList<int> result)
        {
            int[] fields = new int[count];

            int average = sum / count;
            int randomOffset = UnityEngine.Random.Range(-20, 20);
            sum = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                sum += average + UnityEngine.Random.Range(-average / 2, +average / 2);
                fields[i] = sum + randomOffset;
            }

            result = fields;
        }

        public static void SphericalToCartesian(float radius, float degrees, out Vector2 vector)
        {
            vector = Vector2.up * radius;
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = vector.x;
            float ty = vector.y;
            vector.x = (cos * tx) - (sin * ty);
            vector.y = (sin * tx) + (cos * ty);
        }

        public static IEnumerable<(Vector2Int, Vector2Int)> SplitCoreLines(IReadOnlyList<Vector2Int> corePoints,
            Vector2Int startPos)
        {

            List<CircleSegment> circleSegments = new List<CircleSegment>();
            foreach (var t in corePoints)
            {
                var c = new CircleSegment
                {
                    start = startPos,
                    end = t,
                    things = new List<(float, CircleSegment, Vector2Int)>()
                };
                circleSegments.Add(c);
            }

            for (int i = 0; i < corePoints.Count; i++)
            {
                circleSegments[i].left = i == 0 ? circleSegments[^1] : circleSegments[i - 1];
                circleSegments[i].right = i == corePoints.Count - 1 ? circleSegments[0] : circleSegments[i + 1];
            }

            return GenerateLines(circleSegments);
        }

        private static IEnumerable<(Vector2Int, Vector2Int)> GenerateLines(IReadOnlyList<CircleSegment> circleSegments)
        {
            List<(Vector2Int, Vector2Int)> result = new List<(Vector2Int, Vector2Int)>();
            float maxLength = circleSegments[0].GetLength();
            for (int i = 0; i < maxLength / TOKEN_DEFAULT_SECTOR_FRAGMENT_LENGTH - 2; i++)
            {
                foreach (var segment in circleSegments)
                {
                    segment.CreateNextThing();
                }

                foreach (var t in circleSegments)
                {
                    var resultingVector = new Vector2Int();
                    var currentLineSegment = Mathf.Abs(t.currentDistance - t.left.currentDistance) >
                                             Mathf.Abs(t.currentDistance - t.right.currentDistance)
                        ? t.left
                        : t.right;

                    int pos = currentLineSegment.things.Count - 2;
                    if (pos < 0)
                        pos = 0;
                    var resultingTmpVector = Vector2.Lerp(currentLineSegment.start, currentLineSegment.end,
                        currentLineSegment.currentDistance / currentLineSegment.GetLength());
                    resultingVector.x = (int)resultingTmpVector.x;
                    resultingVector.y = (int)resultingTmpVector.y;
                    currentLineSegment.things.Insert(pos,
                        new(t.currentDistance, t, resultingVector));
                }
            }

            foreach (var circleSegment in circleSegments)
            {
                circleSegment.OrderThings();
            }

            foreach (var t in circleSegments)
            {
                result.Add((t.start, t.things[0].Item3));
                result.Add((t.things[^1].Item3, t.end));
                for (int j = 0; j < t.things.Count - 1; j++)
                {
                    result.Add((t.things[j].Item3, t.things[j + 1].Item3));
                    if (t.things[j].Item2 != t)
                    {
                        var end = t.things[j].Item2.things.First(c => Mathf.Approximately(t.things[j].Item1, c.Item1));
                        result.Add(new(t.things[j].Item3, end.Item3));
                    }
                }
            }

            return result;
        }

        public static bool IsOneLineFarMoreLonger(IReadOnlyList<float> lengths, out int result)
        {
            result = -1;
            float sum = lengths.Sum();
            for (int i = 0; i < lengths.Count; i++)
            {
                if (lengths[i] / sum >= TOKEN_MAX_PERCENTAGE_SUPERIORITY)
                {
                    result = i;
                    return true;
                }
            }
            return false;
        }
        
        public static List<Vector2> SortVertices (IEnumerable<Vector2> points,Vector2 center,bool isInverse =false)
        {
            if (isInverse)
                return points.OrderByDescending(c =>
                    ((Mathf.Atan2(c.x - center.x, c.y - center.y) * Mathf.Rad2Deg) + 360) % 360).ToList(); 
            return points.OrderBy(c => ((Mathf.Atan2(c.x - center.x, c.y - center.y) * Mathf.Rad2Deg) + 360) % 360).ToList();
        }

        public static List<Vector2> SortVertices(IEnumerable<Vector2> points, bool isInverse = false)
        {
            Vector2 center = Vector2.zero;
            foreach (var vector2 in points)
            {
                center += vector2;
            }
            center /= points.Count();
            return SortVertices(points, center, isInverse);
        }

        //IMPORTANT
        //TODO: replace this values.

        #region Core Values

        private const float TOKEN_MAX_PERCENTAGE_SUPERIORITY = .9f;

        public const float TOKEN_DEBUG_INITIAL_CRACK_FORCE = 40f;

        public const float TOKEN_DEFAULT_CORE_FORCE_VALUE = 5f;
        public const int TOKEN_DEFAULT_MIN_SECTORS_COUNT = 5;
        public const int TOKEN_DEFAULT_MAX_SECTORS_COUNT = 10;
        public const float TOKEN_DEFAULT_CORE_CIRCLE_RADIUS = 70f;
        //TODO: feature forLater
        //public const float TOKEN_DEFAULT_CORE_SPLIT_RADIUS = 20f;
        public const float TOKEN_DEFAULT_SECTOR_FRAGMENT_LENGTH = 15f;
        
        public const float TOKEN_MINIMAL_LINE_FORCE_VALUE = 5f;
        public const float TOKEN_MAXIMUM_LINE_FORCE_VALUE = 10f;
        public const float TOKEN_DEFAULT_LINE_LENGTH = 15f;

        #endregion
    }

    public class CircleSegment
    {
        public Vector2Int start;
        public Vector2Int end;
        public CircleSegment left;
        public CircleSegment right;
        public float currentDistance => things.Count == 0 ? 0f : things[^1].Item1;

        public List<(float, CircleSegment, Vector2Int)> things;
        public float GetLength() => Vector2Int.Distance(start, end);

        public void CreateNextThing()
        {
            float min = currentDistance + CrackExtensions.TOKEN_DEFAULT_SECTOR_FRAGMENT_LENGTH * .9f;
            float max = currentDistance + CrackExtensions.TOKEN_DEFAULT_SECTOR_FRAGMENT_LENGTH;
            if (things.Count == 0)
            {
                min += CrackExtensions.TOKEN_DEFAULT_SECTOR_FRAGMENT_LENGTH;
                max += CrackExtensions.TOKEN_DEFAULT_SECTOR_FRAGMENT_LENGTH;
            }

            float random = Random.Range(min, max);
            var result = new Vector2Int();
            Vector2 res = Vector2.Lerp(start, end, random / GetLength());
            if (things.Count > 1)
            {
                var perpendicular = Vector2.Perpendicular(end - start).normalized;
                res += perpendicular * Random.Range(-2f, 2f);
            }

            result.x = (int)res.x;
            result.y = (int)res.y;
            things.Add((random, this, result));
        }

        public void OrderThings() => things = things.OrderBy(c => c.Item1).ToList();
    }
}