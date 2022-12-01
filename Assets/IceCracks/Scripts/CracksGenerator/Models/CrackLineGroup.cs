using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Ext = CrackAreaExtensions;

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
