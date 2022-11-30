using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ext = CrackAreaExtensions;
using Random = UnityEngine.Random;

public abstract class CrackArea
{
    public abstract bool IsIntersect(Vector2 point);

    public abstract bool IsIntersectLine(Vector2 sPoint, Vector2 ePoint);

    public abstract bool IsIntersectedWith(CrackArea area);

    public abstract IReadOnlyCollection<(Vector2Int, Vector2Int)> GetPoints();

    public event Action OnInternalStateChanged = delegate { };

    protected virtual void NotifyInternalStateChanged() => OnInternalStateChanged.Invoke();

    public bool HasStateChangedSinceLastPointsGet { get; protected set; }
}

public class CrackCore : CrackArea
{
    public readonly Vector2Int position;
    public readonly float radius;

    private readonly List<CrackLineGroup> connectedLines;
    public IReadOnlyList<CrackLineGroup> ConnectedLines => connectedLines;
    private readonly List<(Vector2Int, Vector2Int)> crackedLines;
    private List<Vector2Int> exitCrackPositions;

    public CrackCore(Vector2Int position, float radius, out IReadOnlyList<CrackLineGroup> generated, float force = 10f)
    {
        HasStateChangedSinceLastPointsGet = true;
        this.position = position;
        this.radius = radius;
        connectedLines = new List<CrackLineGroup>();
        exitCrackPositions = new List<Vector2Int>();
        crackedLines = new List<(Vector2Int, Vector2Int)>();
        GeneratePoints();
        if (force > Ext.TOKEN_DEFAULT_CORE_FORCE_VALUE +
            Ext.TOKEN_MINIMAL_LINE_FORCE_VALUE)
        {
            force -= Ext.TOKEN_DEFAULT_CORE_FORCE_VALUE;
            connectedLines.AddRange(ProlongCrack(force));
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
        HasStateChangedSinceLastPointsGet = false;
        return crackedLines;
    }

    private void GeneratePoints()
    {
        Ext.GenerateValuesWithSum(360,
            ((int)(radius / Ext.TOKEN_DEFAULT_RADIUS) *
             Ext.TOKEN_DEFAULT_SECTORS_COUNT), out var result);
        int k = 0;
        foreach (var p in result)
        {
            Ext.SphericalToCartesian(radius,p,out var vector);
            vector += position;
            exitCrackPositions.Add(new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y)));
        }
        crackedLines.AddRange(Ext.SplitCoreLines(exitCrackPositions, position));
    }
    

    public IEnumerable<CrackLineGroup> ProlongCrack(float force)
    {
        var generatedCracks = new List<CrackLineGroup>();
        if (connectedLines.Count == 0)
            generatedCracks.AddRange(RawProlongCreate(force));
        else
        {
            //TODO: upd!
            float forceCreate = 0f;
            float forceAdditive = force;
            generatedCracks.AddRange(RawProlongCreate(forceCreate));
            ProlongExistedLines(forceAdditive);
        }
        return generatedCracks;
    }

    private void ProlongExistedLines(float force)
    {
        float additiveForce = 0;
        var connectedLinesCopy = new List<CrackLineGroup>();
        connectedLinesCopy.AddRange(connectedLines);
        while (force > Ext.TOKEN_MAXIMUM_LINE_FORCE_VALUE +Ext.TOKEN_MINIMAL_LINE_FORCE_VALUE || connectedLinesCopy.Count > 2)
        {
            additiveForce = Random.Range(Ext.TOKEN_MINIMAL_LINE_FORCE_VALUE, Ext.TOKEN_MAXIMUM_LINE_FORCE_VALUE);
            var selectedLine = connectedLinesCopy[Random.Range(0, connectedLinesCopy.Count)];
            selectedLine.ProlongCrack(additiveForce);
            force -= additiveForce;
            connectedLinesCopy.Remove(selectedLine);
        }
        connectedLinesCopy.First().ProlongCrack(force);
    }

    private IEnumerable<CrackLineGroup> RawProlongCreate(float force)
    {
        var newCracks = new List<CrackLineGroup>();
        var selectedPositions = new List<Vector2Int>();
        while (force>Ext.TOKEN_MINIMAL_LINE_FORCE_VALUE)
        {
            float currentForce =
                Random.Range(Ext.TOKEN_MINIMAL_LINE_FORCE_VALUE, Mathf.Min(Ext.TOKEN_MAXIMUM_LINE_FORCE_VALUE, force));
            force -= currentForce;
            if (force < Ext.TOKEN_MINIMAL_LINE_FORCE_VALUE)
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
            else
            {
                Debug.LogError("Error on generation! No spare point!");
                return newCracks;
            }
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
        sparePositions = sparePositions.OrderByDescending(c => selectedPositions.Sum(k => Vector2Int.Distance(c, k))).ToList();
        while (sparePositions.Count > startSparePosCount * .5f && sparePositions.Count > 2)
        {
            sparePositions.RemoveAt(sparePositions.Count - 1);
        }
        
        return sparePositions[Random.Range(0, sparePositions.Count)];
    }
}

public abstract class CrackLineBasic : CrackArea
{
    public Vector2Int startPoint { get; protected set; }
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
        HasStateChangedSinceLastPointsGet = true;
        this.endPoint = endPoint;
        this.startPoint = startPoint;
        length = Vector2Int.Distance(endPoint, startPoint) / 2f ;
        centerPoint = ((Vector2)startPoint + endPoint) / 2;
        pointsGroup = new List<(Vector2Int, Vector2Int)> { (startPoint, endPoint) };
    }

    public static CrackLine GenerateCrackLineWithForce(ref float force, Vector2Int startPosition, Vector2 direction,bool useExcessForce = false)
    {
        float usedForce = 0f;
        if (useExcessForce)
        {
            usedForce = force;
            force = 0f;
        }
        else
        {
            usedForce = Random.Range(Ext.TOKEN_MINIMAL_LINE_FORCE_VALUE,
                Mathf.Min(Ext.TOKEN_MAXIMUM_LINE_FORCE_VALUE, force));
            force -= usedForce;
        }
        direction.Normalize();
        direction *= usedForce / Ext.TOKEN_MINIMAL_LINE_FORCE_VALUE;
        direction *= Ext.TOKEN_DEFAULT_LINE_LENGTH;
        Vector2 endPointV2 = (startPosition + direction +
                              Vector2.Perpendicular(direction).normalized * Random.Range(-1f, 1f));
        Vector2Int endPoint = new Vector2Int((int)endPointV2.x, (int)endPointV2.y);
        var result = new CrackLine(startPosition, endPoint);
        return result;
    }

    public static List<List<CrackLine>> GenerateMultipleLinesWithForce(float force, Vector2Int startPosition,
        Vector2 direction)
    {
        int maxCount = Mathf.RoundToInt(force / Ext.TOKEN_MINIMAL_LINE_FORCE_VALUE);
        int randomCount = MathExtensions.RandomCountWithExponentialHardnessIncrease(Mathf.Min(maxCount, 3), false);
        List<List<CrackLine>> result = new List<List<CrackLine>>();
        switch (randomCount)
        {
            case 1:
                result.Add(new List<CrackLine>());
                result[0].AddRange(GenerateMultipleLinedCrackLines(force, startPosition, direction));
                break;
            case 2:
            {
                MathExtensions.SplitFloatByTwo(force * 1.5f, out var first, out var second);
                result.Add(new List<CrackLine>());
                result.Add(new List<CrackLine>());
                Vector2 randVal = Vector2.Perpendicular(direction).normalized;
                result[0].AddRange(GenerateMultipleLinedCrackLines(first, startPosition,
                    direction + randVal * Random.Range(-6, -2)));
                result[1].AddRange(GenerateMultipleLinedCrackLines(second, startPosition,
                    direction + randVal * Random.Range(2, 6)));
            }
                break;
            case 3:
            {
                MathExtensions.SplitFloatByThree(force * 2f, out var first, out var second, out var third);result.Add(new List<CrackLine>());
                result.Add(new List<CrackLine>());
                result.Add(new List<CrackLine>());
                result.Add(new List<CrackLine>());
                Vector2 randVal = Vector2.Perpendicular(direction).normalized;
                result[0].AddRange(GenerateMultipleLinedCrackLines(first, startPosition,
                    direction + randVal * Random.Range(-7, -3)));
                result[1].AddRange(GenerateMultipleLinedCrackLines(second, startPosition,
                    direction + randVal * Random.Range(-2, 2)));
                result[2].AddRange(GenerateMultipleLinedCrackLines(second, startPosition,
                    direction + randVal * Random.Range(2, -7)));
            }
                break;
        }
        
        return result;
    }

    public static IEnumerable<CrackLine> GenerateMultipleLinedCrackLines(float force, Vector2Int startPosition,
        Vector2 direction)
    {
        List<CrackLine> result = new List<CrackLine>();
        while (force > Ext.TOKEN_MINIMAL_LINE_FORCE_VALUE * 2f)
        {
            //TODO: possible infinite loop!
            force *= 1.1f;
            result.Add(GenerateCrackLineWithForce(ref force, startPosition, direction));
            var lastRes = result.Last();
            startPosition = lastRes.endPoint;
            direction = lastRes.Direction;
        }

        result.Add(GenerateCrackLineWithForce(ref force, startPosition, direction, true));
        return result;
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

    public override IReadOnlyCollection<(Vector2Int, Vector2Int)> GetPoints()
    {
        HasStateChangedSinceLastPointsGet = false;
        return pointsGroup;
    }

    public float GetLength() => Vector2Int.Distance(startPoint, endPoint);
}

public class CrackLineGroup : CrackLineBasic
{
    private List<List<CrackLine>> lines;
    public int GetLinesAmount() => lines.Count;

    public IReadOnlyList<CrackLine> Lines
    {
        get
        {
            cachedLines ??= lines.SelectMany(c => c).ToList();
            return cachedLines;
        }
    }
    private List<CrackLine> cachedLines;

    public override Vector2 Direction => Lines.Aggregate(Vector2.zero, (s, v) => s + v.Direction).normalized;
    public override float Length => Lines.Sum(c => c.Length);

    public static CrackLineGroup CreateGroupByForceDirectionStartPosition(float force,Vector2 direction,Vector2Int startPosition)
    {
        CrackLineGroup result = new CrackLineGroup
        {
            HasStateChangedSinceLastPointsGet = true,
            lines = new List<List<CrackLine>>(),
            startPoint = startPosition
        };
        //TODO: refactor this
        if (force > Ext.TOKEN_MINIMAL_LINE_FORCE_VALUE * 2 || Random.Range(0f, 1f) > .6f)
        {
            result.lines.AddRange(CrackLine.GenerateMultipleLinesWithForce(force, startPosition, direction));
        }
        else
        {
            result.lines.Add(new List<CrackLine>());
            result.lines[0].AddRange(CrackLine.GenerateMultipleLinedCrackLines(force, startPosition, direction));
        }
        return result;
    }

    public void ProlongCrack(float force)
    {
        //TODO: dont forget to notify internal state change if added new list of "lines"!
        if (lines.Count == 3)
        {
            
        }
        NotifyInternalStateChanged();
    }
    
    public override bool IsIntersect(Vector2 point)
    {
        return Lines.Any(c => c.IsIntersect(point));
    }

    public override bool IsIntersectLine(Vector2 sPoint, Vector2 ePoint)
    {
        return Lines.Any(c => c.IsIntersectLine(sPoint, ePoint));
    }

    public override bool IsIntersectedWith(CrackArea area) => area switch
    {
        CrackCore core => Lines.Any(core.IsIntersected),
        CrackLine line => Lines.Any(line.IsIntersected),
        CrackLineGroup group => group.Lines.Any(c => Lines.Any(k=>k.IsIntersected(c))),
        _ => throw new NotImplementedException()
    };

    protected override void NotifyInternalStateChanged()
    {
        cachedLines.Clear();
        cachedLines = null;
        HasStateChangedSinceLastPointsGet = true;
        base.NotifyInternalStateChanged();
    }

    public override IReadOnlyCollection<(Vector2Int, Vector2Int)> GetPoints()
    {
        HasStateChangedSinceLastPointsGet = false;
        return Lines.SelectMany(c => c.GetPoints()).ToList();
    }

    public float GetLength() => Lines.Sum(c => c.GetLength());
}
