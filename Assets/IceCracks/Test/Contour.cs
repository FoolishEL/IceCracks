using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace IceCracks.Utilities
{
    public class Contour
    {
        private List<Vector2> points;
        private Vector2 center;
        private Bounds bounds;

        public Contour(IReadOnlyList<Vector2> initialPoints)
        {
            center = Vector2.zero;
            foreach (var initialPoint in initialPoints)
            {
                center += initialPoint;
            }
            center /= initialPoints.Count;
            RawCreate(initialPoints, center);
        }

        public Contour(IReadOnlyList<Vector2> initialPoints, Vector2 center)
        {
            this.center = center;
            RawCreate(initialPoints, center);
        }

        private void RawCreate(IEnumerable<Vector2> initialPoints,Vector2 center)
        {
            bounds = new Bounds
            {
                center = center
            };
            foreach (var point in initialPoints)
            {
                bounds.Encapsulate(point);
            }
            points = SortVertices(initialPoints, center);
        }
        
        public static List<Vector2> SortVertices (IEnumerable<Vector2> points,Vector2 center,bool isInverse =false)
        {
            if (isInverse)
                return points.OrderByDescending(c =>
                    ((Mathf.Atan2(c.x - center.x, c.y - center.y) * Mathf.Rad2Deg) + 360) % 360).ToList(); 
            return points.OrderBy(c => ((Mathf.Atan2(c.x - center.x, c.y - center.y) * Mathf.Rad2Deg) + 360) % 360).ToList();
        }

        public bool IsIntersected(Contour other)
        {
            if (!bounds.Intersects(other.bounds))
                return false;
            return false;
        }
    }
}