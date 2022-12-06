using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IceCracks.CracksGeneration.Models
{
    using Math;
    using Extensions;

    public class CrackLine : CrackLineBasic
    {
        public readonly Vector2Int endPoint;
        public override Vector2 direction => ((Vector2)(endPoint - startPoint)).normalized;
        public override float Length => length;

        private readonly float length;
        private readonly Vector2 centerPoint;
        private List<(Vector2Int, Vector2Int)> pointsGroup;

        private CrackLine(Vector2Int startPoint, Vector2Int endPoint, float force)
        {
#if UNITY_EDITOR
            if (startPoint == endPoint || Mathf.Approximately(0f, force))
            {
                throw new ArgumentException();
            }
#endif
            hasStateChangedSinceLastPointsGet = true;
            this.endPoint = endPoint;
            this.startPoint = startPoint;
            length = Vector2Int.Distance(endPoint, startPoint) / 2f;
            centerPoint = ((Vector2)startPoint + endPoint) / 2;
            pointsGroup = new List<(Vector2Int, Vector2Int)> { (startPoint, endPoint) };
            storedForce = force;
        }

        public static CrackLine GenerateCrackLineWithForce(ref float force, Vector2Int startPosition, Vector2 direction,
            bool useExcessForce = false)
        {
            float usedForce;
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

            direction = MathExtensions.RandomVectorRotationInAngleRangeWithHole(direction, 2f, .5f);
            direction.Normalize();
            direction *= usedForce / CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE;
            direction *= CrackExtensions.TOKEN_DEFAULT_LINE_LENGTH;
            Vector2 endPointV2 = startPosition + direction;
            return new CrackLine(startPosition, new Vector2Int((int)endPointV2.x, (int)endPointV2.y), usedForce);
        }

        public static List<List<CrackLine>> GenerateMultipleLinesWithForce(float force, Vector2Int startPosition,
            Vector2 direction)
        {
            int maxCount = Mathf.FloorToInt(force / CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE);
            int randomCount = MathExtensions.RandomCountWithExponentialHardnessIncrease(Mathf.Min(maxCount, 3), false);
            List<List<CrackLine>> result = new List<List<CrackLine>>();
            switch (randomCount)
            {
                case 2:
                {
                    MathExtensions.SplitFloatByTwo(force * 1.5f, out var first, out var second);
                    result.Add(new List<CrackLine>());
                    result.Add(new List<CrackLine>());
                    result[0].AddRange(GenerateMultipleLinedCrackLines(first, startPosition,
                        MathExtensions.RandomVectorRotationInAngleRange(direction, -6f, -2f)));
                    result[1].AddRange(GenerateMultipleLinedCrackLines(second, startPosition,
                        MathExtensions.RandomVectorRotationInAngleRange(direction, 2f, 2f)));
                }
                    break;
                case 3:
                {
                    MathExtensions.SplitFloatByThree(force * 2f, out var first, out var second, out var third);
                    result.Add(new List<CrackLine>());
                    result.Add(new List<CrackLine>());
                    result.Add(new List<CrackLine>());
                    result.Add(new List<CrackLine>());
                    result[0].AddRange(GenerateMultipleLinedCrackLines(first, startPosition,
                        MathExtensions.RandomVectorRotationInAngleRange(direction, -7f, -3f)));
                    result[1].AddRange(GenerateMultipleLinedCrackLines(second, startPosition,
                        MathExtensions.RandomVectorRotationInAngleRange(direction, -2f, 2f)));
                    result[2].AddRange(GenerateMultipleLinedCrackLines(third, startPosition,
                        MathExtensions.RandomVectorRotationInAngleRange(direction, 2f, 7f)));
                }
                    break;
                default:
                    result.Add(new List<CrackLine>());
                    result[0].AddRange(GenerateMultipleLinedCrackLines(force, startPosition,
                        MathExtensions.RandomVectorRotationInAngleRangeWithHole(direction, 2f, .5f)));
                    break;
            }

            return result;
        }

        public static IEnumerable<CrackLine> GenerateMultipleLinedCrackLines(float force, Vector2Int startPosition,
            Vector2 direction)
        {
            List<CrackLine> result = new List<CrackLine>();
            do
            {
                //TODO: possible infinite loop!
                force *= 1.1f;
                var generatedCrackLine = GenerateCrackLineWithForce(ref force, startPosition, direction);
                result.Add(generatedCrackLine);
                startPosition = generatedCrackLine.endPoint;
                direction = generatedCrackLine.direction;
            } while (force > CrackExtensions.TOKEN_MAXIMUM_LINE_FORCE_VALUE * 2f);
            if (force >= CrackExtensions.TOKEN_MINIMAL_LINE_FORCE_VALUE)
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
            CrackLineGroup group => group.Lines.Any(c => c.IsIntersected(this)),
            _ => throw new NotImplementedException()
        };

        public override IReadOnlyCollection<(Vector2Int, Vector2Int)> GetPoints()
        {
            hasStateChangedSinceLastPointsGet = false;
            return pointsGroup;
        }

        public float GetLength() => Vector2Int.Distance(startPoint, endPoint);
    }
}