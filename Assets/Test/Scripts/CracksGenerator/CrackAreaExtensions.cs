using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CrackAreaExtensions
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

    public static void GenerateValuesWithSum(int sum,int count,out IReadOnlyList<int> result)
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

    public static IEnumerable<(Vector2Int,Vector2Int)> SplitCoreLines(List<Vector2Int> corePoints,Vector2Int startPos)
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

    private static IEnumerable<(Vector2Int, Vector2Int)> GenerateLines(List<CircleSegment> circleSegments)
    {
        List<(Vector2Int, Vector2Int)> result = new List<(Vector2Int, Vector2Int)>();
        float maxLength = circleSegments[0].GetLength();
        for (int i = 0; i < maxLength / TOKEN_DEFAULT_SECTOR_FRAGMENT_LENGTH - 1; i++)
        {
            circleSegments.ForEach(c=>c.CreateNextThing());
            foreach (var t in circleSegments)
            {
                var _result = new Vector2Int();
                Vector2 res;
                if (Mathf.Abs(t.currentDistance - t.left.currentDistance) >
                    Mathf.Abs(t.currentDistance - t.right.currentDistance))
                {
                    res = Vector2.Lerp(t.left.start, t.left.end, t.left.currentDistance / t.left.GetLength());
                    _result.x = (int)res.x;
                    _result.y = (int)res.y;
                    int pos = t.left.things.Count - 2;
                    if (pos < 0)
                        pos = 0;
                    t.left.things.Insert(pos,
                        new(t.currentDistance, t, _result));
                }
                else
                {
                    int pos = t.right.things.Count - 2;
                    if (pos < 0)
                        pos = 0;
                    res = Vector2.Lerp(t.right.start, t.right.end, t.right.currentDistance / t.right.GetLength());
                    _result.x = (int)res.x;
                    _result.y = (int)res.y;
                    t.right.things.Insert(pos,
                        new(t.currentDistance, t, _result));
                }
            }
        }
        circleSegments.ForEach(t => t.things = t.things.OrderBy(c => c.Item1).ToList());
        foreach (var t in circleSegments)
        {
            result.Add((t.start, t.things[0].Item3));
            for (int j = 0; j < t.things.Count-1; j++)
            {
                result.Add((t.things[j].Item3, t.things[j + 1].Item3));
                if (t.things[j].Item2 != t)
                {
                    var end = t.things[j].Item2.things.First(c => Mathf.Approximately(t.things[j].Item1, c.Item1));
                    result.Add(new(t.things[j].Item3, end.Item3));
                }
            }
        }
        // for (int i = 0; i < circleSegments.Count - 1; i++)
        // {
        //     result.Add((circleSegments[i].end, circleSegments[i + 1].end));
        //     result.Add((circleSegments[i].start, circleSegments[i].end));
        // }
        //
        // result.Add((circleSegments[^1].end, circleSegments[0].end));
        // result.Add((circleSegments[^1].start, circleSegments[^1].end));
        return result;
    }
    
    //IMPORTANT
    #region MyRegion
    public const float TOKEN_DEFAULT_CORE_FORCE_VALUE = 5f;
    public const float TOKEN_DEFAULT_LINE_FORCE_VALUE = 5f;
    public const float TOKEN_DEFAULT_RADIUS = 40f;
    public const int TOKEN_DEFAULT_SECTORS_COUNT = 8;
    public const float TOKEN_DEFAULT_SECTOR_FRAGMENT_LENGTH = TOKEN_DEFAULT_RADIUS / 5f;
    #endregion
}

public class CircleSegment
{
    public Vector2Int start;
    public Vector2Int end;
    public CircleSegment left;
    public CircleSegment right;
    public float currentDistance => things.Count == 0 ? 0f : things[^1].Item1;

    public Vector2Int GetStartPosition()
    {
        if(things.Count == 1 || things.Count == 0)
            return start;
        var result = new Vector2Int();
        Vector2 res = Vector2.Lerp(start, end, currentDistance / GetLength());
        result.x = (int)res.x;
        result.y = (int)res.y;
        return result;
        
    }

    public List<(float,CircleSegment,Vector2Int)> things;
    public float GetLength() => Vector2Int.Distance(start, end);

    public void CreateNextThing()
    {
        float min = currentDistance + CrackAreaExtensions.TOKEN_DEFAULT_SECTOR_FRAGMENT_LENGTH * .9f;
        float max = currentDistance + CrackAreaExtensions.TOKEN_DEFAULT_SECTOR_FRAGMENT_LENGTH;
        float random = Random.Range(min, max);
        var result = new Vector2Int();
        Vector2 res = Vector2.Lerp(start, end, random / GetLength());
        result.x = (int)res.x;
        result.y = (int)res.y;
        things.Add((random, this, result));
    }
}
