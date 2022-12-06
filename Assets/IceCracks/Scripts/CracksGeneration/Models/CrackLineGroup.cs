using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IceCracks.CracksGeneration.Models
{
    using Math;
    using Extensions;

    public class CrackLineGroup : CrackLineBasic
    {
        private Vector2 initialDirection;
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

        public override Vector2 direction => Lines.Aggregate(Vector2.zero, (s, v) => s + v.direction).normalized;
        public override float Length => Lines.Sum(c => c.Length);

        public static CrackLineGroup CreateGroupByForceDirectionStartPosition(float force, Vector2 direction,
            Vector2Int startPosition)
        {
            CrackLineGroup result = new CrackLineGroup
            {
                hasStateChangedSinceLastPointsGet = true,
                lines = new List<List<CrackLine>>(),
                startPoint = startPosition,
                initialDirection = direction,
            };
            //TODO: refactor this
            if (force > CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE * 2 ||
                MathExtensions.GetRandomWithPercent(.6f))
            {
                result.lines.AddRange(CrackLine.GenerateMultipleLinesWithForce(force, startPosition, direction));
            }
            else
            {
                result.lines.Add(new List<CrackLine>());
                result.lines[0].AddRange(CrackLine.GenerateMultipleLinedCrackLines(force, startPosition, direction));
            }
            result.UpdateStoredForce();
            return result;
        }

        private void UpdateStoredForce()
        {
            storedForce = 0f;
            lines.ForEach(c => c.ForEach(k => storedForce += k.storedForce));
        }

        public void ProlongCrack(float force)
        {
            var lengths = lines.Select(c => c.Sum(k => k.Length)).ToList();
            if (lines.Count >= 2)
            {
                if (CrackExtensions.IsOneLineFarMoreLonger(lengths, out var resultIfOneLonger) && lines.Count < 4)
                {
                    CreateBranchFromSelectedLine(resultIfOneLonger, force);
                }
                else
                {
                    List<CrackLine> selectedLine;
                    if (MathExtensions.SelectOneFromManyByPercents(lengths, out var result))
                    {
                        selectedLine = lines[result];
                    }
                    else
                    {
                        Debug.LogError("Error in selected start position to prolong!");
                        if (lines.Count > 0)
                            selectedLine = lines[^1];
                        else
                        {
#if UNITY_EDITOR
                            throw new ArithmeticException();
#else
                            return;
#endif
                        }
                    }
                    var lastLine = selectedLine.Last();
                    selectedLine.AddRange(
                        CrackLine.GenerateMultipleLinedCrackLines(force, lastLine.endPoint, lastLine.direction));
                }
            }
            else
            {
                //TODO: magical number "3"!
                if (Length > CrackExtensions.TOKEN_DEFAULT_LINE_LENGTH * 3)
                {
                    lines.Add(new List<CrackLine>());
                    lines[^1].AddRange(
                        CrackLine.GenerateMultipleLinedCrackLines(force, startPoint, initialDirection));
                }
                else
                {
                    if (Length == 0)
                    {
                        lines.Add(new List<CrackLine>());
                        lines[0].AddRange(
                            CrackLine.GenerateMultipleLinedCrackLines(force, startPoint, initialDirection));
                    }
                    else
                    {
                        //TODO: here was nre, i fixed somehow, but not shure!
                        CrackLine lastLine = lines[0][^1];
                        lines[0].AddRange(
                            CrackLine.GenerateMultipleLinedCrackLines(force, lastLine.endPoint, lastLine.direction));
                    }
                }
            }
            UpdateStoredForce();
            NotifyInternalStateChanged();
        }

        private void CreateBranchFromSelectedLine(int lineId, float force)
        {
            var currentLine = lines[lineId];
            /*
            float maxLength = lines.Where(g => g != currentLine).Max(c => c.Sum(k => k.Length));
            float lengthSum = 0f;
            */
            int startPositionId = currentLine.Count - 2;
            if (startPositionId < 0)
                startPositionId = currentLine.Count - 2;
            /*
            for (int i = 0; i < currentLine.Count; i++)
            {
                if (lengthSum > maxLength)
                {
                    startPositionId = i - 1;
                    break;
                }
            
                lengthSum += currentLine[i].GetLength();
            }
            
            if (startPositionId == -1)
            {
                startPositionId = currentLine.Count - 2;
            }
            */

            var direction =
                MathExtensions.RandomVectorRotationInAngleRangeWithHole(currentLine[startPositionId].direction, 5f, 2f);
            direction.Normalize();

            lines.Add(new List<CrackLine>());
            lines[^1].AddRange(CrackLine.GenerateMultipleLinedCrackLines(force, currentLine[startPositionId].startPoint,
                direction));
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
            CrackLineGroup group => group.Lines.Any(c => Lines.Any(k => k.IsIntersected(c))),
            _ => throw new NotImplementedException()
        };

        protected override void NotifyInternalStateChanged()
        {
            cachedLines?.Clear();
            cachedLines = null;
            hasStateChangedSinceLastPointsGet = true;
            base.NotifyInternalStateChanged();
        }

        public override IReadOnlyCollection<(Vector2Int, Vector2Int)> GetPoints()
        {
            hasStateChangedSinceLastPointsGet = false;
            return Lines.SelectMany(c => c.GetPoints()).ToList();
        }

        public float GetLength() => Lines.Sum(c => c.GetLength());
    }
}