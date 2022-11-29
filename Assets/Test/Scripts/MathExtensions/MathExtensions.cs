using UnityEngine;

public static class MathExtensions
{
    public static int RandomCountWithExponentialHardnessIncrease(int maxValueInclusive,bool canBeZero = true)
    {
        //TODO: need testing!
        if (canBeZero)
            maxValueInclusive--;
        float rndValue = Random.Range(0f, Mathf.Exp(maxValueInclusive));
        int result = Mathf.FloorToInt(Mathf.Log(rndValue));
        if (canBeZero)
            result++;
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
}
