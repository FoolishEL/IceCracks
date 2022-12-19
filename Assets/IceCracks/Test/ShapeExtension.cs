using System.Collections;
using System.Collections.Generic;
using Jobberwocky.GeometryAlgorithms.Examples.Data;
using UnityEngine;

public static class ShapeExtension
{
    public static bool IsIntersectsWith(this Shape shape, Shape other)
    {
        var shapePoints = shape.GetAllPoints();
        Bounds firstBounds = new Bounds
        {
            center = shapePoints[0]
        };
        foreach (var shapePoint in shapePoints)
        {
            firstBounds.Encapsulate(shapePoint);
        }
        
        var otherShapePoints = other.GetAllPoints();
        Bounds secondBounds = new Bounds
        {
            center = otherShapePoints[0]
        };
        foreach (var shapePoint in otherShapePoints)
        {
            secondBounds.Encapsulate(shapePoint);
        }
        if (!firstBounds.Intersects(secondBounds))
            return false;
        
        return false;
    }
}
