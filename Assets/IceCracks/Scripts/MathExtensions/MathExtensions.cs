using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class MathExtensions
{
    public static int RandomCountWithExponentialHardnessIncrease(int maxValueInclusive,bool canBeZero = true)
    {
        //TODO: need testing!
        float rndValue = Random.Range(Mathf.Exp(canBeZero ? 0 : 1) + .1f, Mathf.Exp(maxValueInclusive + 1)) - .1f;
        int result = Mathf.FloorToInt(Mathf.Log(rndValue));
        if (result < 1)
        {
            Debug.LogError("Error!");
        }
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
    
    public static void SplitFloatByThree(float initialValue, out float firstResult, out float secondResult,out float thirdResult)
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

    public static void SelectOneFromManyByPercents(IReadOnlyList<float> values, out int idOfSelected)
    {
        idOfSelected = -1;
        var tempPercents = new List<float>();
        tempPercents.AddRange(values);
        var sum = tempPercents.Sum();
        if (Mathf.Approximately(sum,1f))
        {
            tempPercents.ForEach(c =>c /= sum);
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
    }
}
