using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IceCracks.CracksGeneration.Models
{
    using Extensions;

    public class CrackLine : CrackLineBasic
    {
        public readonly Vector2Int startPoint;
        public readonly Vector2Int endPoint;
        public override Vector2 Direction => ((Vector2)(endPoint - startPoint)).normalized;
        public override float Length => length;
    
        private readonly float length;
        private readonly Vector2 centerPoint;
        private List<(Vector2Int, Vector2Int)> pointsGroup;

        public CrackLine(Vector2Int startPoint, Vector2Int endPoint,float force)
        {
            HasStateChangedSinceLastPointsGet = true;
            this.endPoint = endPoint;
            this.startPoint = startPoint;
            length = Vector2Int.Distance(endPoint, startPoint) / 2f ;
            centerPoint = ((Vector2)startPoint + endPoint) / 2;
            pointsGroup = new List<(Vector2Int, Vector2Int)> { (startPoint, endPoint) };
            storedForce = force;
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
                usedForce = Random.Range(CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE,
                    Mathf.Min(CrackExtensions.TOKEN_MAXIMUM_LINE_FORCE_VALUE, force));
                force -= usedForce;
            }
            direction.Normalize();
            direction *= usedForce / CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE;
            direction *= CrackExtensions.TOKEN_DEFAULT_LINE_LENGTH;
            Vector2 endPointV2 = (startPosition + direction +
                                  Vector2.Perpendicular(direction).normalized * Random.Range(-1f, 1f));
            Vector2Int endPoint = new Vector2Int((int)endPointV2.x, (int)endPointV2.y);
            var result = new CrackLine(startPosition, endPoint, usedForce);
            return result;
        }

        public static List<List<CrackLine>> GenerateMultipleLinesWithForce(float force, Vector2Int startPosition,
            Vector2 direction)
        {
            int maxCount = Mathf.RoundToInt(force / CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE);
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
            while (force > CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE * 2f)
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
            //TODO: optimize this!
            for (float step = .0f; step <= 1f; step += .02f)
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
}
