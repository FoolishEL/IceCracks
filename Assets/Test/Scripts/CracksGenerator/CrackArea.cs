using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public abstract class CrackArea
{
    public abstract bool IsIntersect(Vector2 point);

    public abstract bool IsIntersectLine(Vector2 sPoint, Vector2 ePoint);

    public abstract bool IsIntersectedWith(CrackArea area);

    public abstract IReadOnlyCollection<(Vector2Int, Vector2Int)> GetPoints();

    public event Action OnInternalStateChanged = delegate { };

    protected void NotifyInternalStateChanged() => OnInternalStateChanged.Invoke();
}

public class CrackCore : CrackArea
{
    public readonly Vector2Int position;
    public readonly float radius;

    private readonly List<CrackLineGroup> connectedLines;
    public IReadOnlyList<CrackLineGroup> ConnectedLines => connectedLines;
    private readonly List<(Vector2Int, Vector2Int)> crackedLines;

    public CrackCore(Vector2Int position, float radius, out IReadOnlyList<CrackLineGroup> generated, float force = 10f)
    {
        this.position = position;
        this.radius = radius;
        connectedLines = new List<CrackLineGroup>();
        if (force > CrackAreaExtensions.TOKEN_DEFAULT_CORE_FORCE_VALUE +
            CrackAreaExtensions.TOKEN_DEFAULT_LINE_FORCE_VALUE)
        {
            force -= CrackAreaExtensions.TOKEN_DEFAULT_CORE_FORCE_VALUE;
            connectedLines.AddRange(ProlongCrack(force));
        }
        generated = connectedLines;
        crackedLines = new List<(Vector2Int, Vector2Int)>();
    }
    
    public override bool IsIntersect(Vector2 point)
    {
        return Vector2.Distance(point, position) <= radius;
    }

    public override bool IsIntersectLine(Vector2 sPoint, Vector2 ePoint)
    {
        if (IsIntersect(sPoint) || IsIntersect(ePoint))
            return true;
        Vector2 center = ((Vector2)sPoint + ePoint) / 2;
        float distanceFromCenter = Vector2.Distance(center, position);
        float distanceFromStart = Vector2.Distance(sPoint,position);
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
        return crackedLines;
    }

    private void GeneratePoints()
    {
        CrackAreaExtensions.GenerateValuesWithSum(360,
            ((int)(radius / CrackAreaExtensions.TOKEN_DEFAULT_RADIUS) *
             CrackAreaExtensions.TOKEN_DEFAULT_SECTORS_COUNT), out var result);
        List<Vector2Int> positions = new List<Vector2Int>();
        int k = 0;
        foreach (var p in result)
        {
            CrackAreaExtensions.SphericalToCartesian(radius,p,out var vector);
            vector += position;
            positions.Add(new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y)));
        }

        crackedLines.AddRange(CrackAreaExtensions.SplitCoreLines(positions, position));
    }
    

    public IEnumerable<CrackLineGroup> ProlongCrack(float force)
    {
        var newCracks = new List<CrackLineGroup>();

        NotifyInternalStateChanged();
        return newCracks;
    }
}

public abstract class CrackLineBasic : CrackArea
{
    public readonly Vector2Int startPoint;
    public readonly Vector2Int endPoint;
    public abstract Vector2 Direction { get; }
    public abstract float Length { get; }
}

public class CrackLine : CrackLineBasic
{
    public readonly Vector2Int startPoint;
    public readonly Vector2Int endPoint;
    public override Vector2 Direction => ((Vector2)(endPoint - startPoint)).normalized;
    public override float Length => length;
    
    private readonly float length;
    private readonly Vector2 centerPoint;
    private List<(Vector2Int, Vector2Int)> pointsGroup;

    public CrackLine(Vector2Int startPoint, Vector2Int endPoint)
    {
        this.endPoint = endPoint;
        this.startPoint = startPoint;
        length = Vector2Int.Distance(endPoint, startPoint) / 2f ;
        centerPoint = ((Vector2)startPoint + endPoint) / 2;
        pointsGroup = new List<(Vector2Int, Vector2Int)>();
        pointsGroup.Add((startPoint, endPoint));
    }
    
    public override bool IsIntersect(Vector2 point)
    {
        return Vector2.Distance(point, centerPoint) <= length + Mathf.Sqrt(2);
    }

    public override bool IsIntersectLine(Vector2 sPoint, Vector2 ePoint)
    {
        if (IsIntersect(ePoint) || IsIntersect(sPoint))
            return true;
        //TODO: оптимизировать!
        for (float step = .0f; step <= 1f; step += 0.02f)
        {
            if (IsIntersect(Vector2.Lerp(sPoint, ePoint, step)))
                return true;
        }
        return false;
    }

    public override bool IsIntersectedWith(CrackArea area) => area switch
    {
        CrackCore core => core.IsIntersected(this),
        CrackLine line => this.IsIntersected(line),
        CrackLineGroup group => group.Lines.Any(c=>c.IsIntersected(this)),
        _ => throw new NotImplementedException()
    };

    public override IReadOnlyCollection<(Vector2Int, Vector2Int)> GetPoints() => pointsGroup;

    public float GetLength() => Vector2Int.Distance(startPoint, endPoint);
}

public class CrackLineGroup : CrackLineBasic
{
    private List<CrackLine> lines;
    public IReadOnlyList<CrackLine> Lines => lines;

    public override Vector2 Direction => lines.Aggregate(Vector2.zero, (s, v) => s + v.Direction).normalized;
    public override float Length => lines.Sum(c => c.Length);

    public CrackLineGroup(CrackLine initialLine)
    {
        lines = new List<CrackLine>();
        lines.Add(initialLine);
    }
    
    public override bool IsIntersect(Vector2 point)
    {
        return lines.Any(c => c.IsIntersect(point));
    }

    public override bool IsIntersectLine(Vector2 sPoint, Vector2 ePoint)
    {
        return lines.Any(c => c.IsIntersectLine(sPoint, ePoint));
    }

    public override bool IsIntersectedWith(CrackArea area) => area switch
    {
        CrackCore core => lines.Any(core.IsIntersected),
        CrackLine line => lines.Any(line.IsIntersected),
        CrackLineGroup group => group.Lines.Any(c => lines.Any(k=>k.IsIntersected(c))),
        _ => throw new NotImplementedException()
    };

    public override IReadOnlyCollection<(Vector2Int, Vector2Int)> GetPoints() =>
        lines.SelectMany(c => c.GetPoints()).ToList();

    public float GetLength() => lines.Sum(c => c.GetLength());
}
