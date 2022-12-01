using System;
using System.Collections.Generic;
using UnityEngine;

namespace IceCracks.CracksGeneration.Models
{
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

    public abstract class CrackLineBasic : CrackArea
    {
        public Vector2Int startPoint { get; protected set; }
        public abstract Vector2 Direction { get; }
        public abstract float Length { get; }

        public float storedForce { get; protected set; }
    }
}