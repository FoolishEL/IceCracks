using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IceCracks.Math
{
    public static class MathExtensions
    {
        public static int RandomCountWithExponentialHardnessIncrease(int maxValueInclusive, bool canBeZero = true)
        {
            //TODO: need testing!
            float rndValue = Random.Range(Mathf.Exp(canBeZero ? 0 : 1) + .1f, Mathf.Exp(maxValueInclusive + 1)) - .1f;
            int result = Mathf.FloorToInt(Mathf.Log(rndValue));
#if UNITY_EDITOR
            if (result < 1)
            {
                Debug.LogError("Error!");
            }
#endif
            return result;
        }

        public static void SplitFloatByTwo(float initialValue, out float firstResult, out float secondResult)
        {
            float startValue = initialValue;
            var vector = Random.insideUnitCircle;
            initialValue /= 2f;
            var mainPart = Vector2.one.normalized;
            float additivePart = mainPart.x * mainPart.x * (startValue - initialValue);
            firstResult = vector.x * vector.x * initialValue + additivePart;
            secondResult = vector.y * vector.y * initialValue + additivePart;
        }

        public static void SplitFloatByThree(float initialValue, out float firstResult, out float secondResult,
            out float thirdResult)
        {
            float startValue = initialValue;
            var vector = Random.insideUnitSphere;
            initialValue /= 2f;
            var mainPart = Vector3.one.normalized;
            float additivePart = mainPart.x * mainPart.x * (startValue - initialValue);
            firstResult = vector.x * vector.x * initialValue + additivePart;
            secondResult = vector.y * vector.y * initialValue + additivePart;
            thirdResult = vector.z * vector.z * initialValue + additivePart;
        }

        /// <summary>
        /// Get random true/false depends on percent probability.
        /// throw if value is out of range
        /// </summary>
        /// <param name="percent">0f..1f</param>
        public static bool GetRandomWithPercent(float percent)
        {
            if (percent < 0f || percent > 1f)
            {
                throw new InvalidDataException();
            }

            return Random.Range(Mathf.Epsilon, 1f) <= percent;
        }

        /// <summary>
        /// Get random true/false depends on percent probability.
        /// throw if value is out of range 
        /// </summary>
        /// <param name="percent">0..100</param>
        public static bool GetRandomWithPercent(int percent)
        {
            if (percent < 0 || percent > 100)
            {
                throw new InvalidDataException();
            }

            return Random.Range(1, 101) <= percent;
        }

        public static bool SelectOneFromManyByPercents(IReadOnlyList<float> values, out int idOfSelected)
        {
            idOfSelected = -1;
            var tempPercents = new List<float>();
            tempPercents.AddRange(values);
            var sum = tempPercents.Sum();
            if (!Mathf.Approximately(sum, 1f))
            {
                for (int i = 0; i < tempPercents.Count; i++)
                {
                    tempPercents[i] /= sum;
                }
            }

            for (int i = 1; i < tempPercents.Count; i++)
            {
                tempPercents[i] += tempPercents[i - 1];
            }

            float random = Random.Range(0f, 1f);
            for (int i = 0; i < tempPercents.Count; i++)
            {
                if (random < tempPercents[i])
                {
                    idOfSelected = i;
                    break;
                }
            }

            return idOfSelected != -1;
        }

        /// <summary>
        /// Rotate vector in random angle in range [- angle .. angle] 
        /// </summary>
        /// <returns>returns new Vector2 with rotation</returns>
        public static Vector2 RandomVectorRotationInAngleRange(Vector2 initialVector, float angle)
        {
            if (angle < 0f)
                angle *= -1f;
            return RandomVectorRotationInAngleRange(initialVector, -angle, angle);
        }

        public static Vector2 RandomVectorRotationInAngleRangeWithHole(Vector2 initialVector, float angle,
            float innerAngle)
        {
            if (angle < 0f)
                angle *= -1f;
            if (innerAngle < 0)
                innerAngle *= -1f;
            if (innerAngle > angle)
                (innerAngle, angle) = (angle, innerAngle);
            bool isLeft = GetRandomWithPercent(.5f);
            return RandomVectorRotationInAngleRange(initialVector, isLeft ? -angle : innerAngle,
                isLeft ? -innerAngle : angle);
        }

        public static Vector2 RandomVectorRotationInAngleRange(Vector2 initialVector, float startAngle, float endAngle)
        {
            if (startAngle > endAngle)
                (startAngle, endAngle) = (endAngle, startAngle);
            return initialVector.Rotate(Random.Range(startAngle, endAngle));
        }

        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = cos * tx - sin * ty;
            v.y = sin * tx + cos * ty;
            return v;
        }

        public static float Rebase(float initialValue, float minInitial, float maxInitial, float minNew, float maxNew)
        {
            float percentage = (initialValue - minInitial) / (maxInitial - minInitial);
            return Mathf.Lerp(minNew, maxNew, percentage);
        }

        public static Vector2 Rebase(Vector2 initialVector, Vector2 minInitial,Vector2 maxInitial,Vector2 minNew, Vector2 maxNew)
        {
            return Vector2.down;
        }
    }
}