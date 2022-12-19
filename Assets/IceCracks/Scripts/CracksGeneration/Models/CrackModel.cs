using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IceCracks.CracksGeneration.Models
{
    using Utilities;
    using Math;
    using Extensions;

    public class CrackModel
    {
        private Vector2Int sizeOfTexture;
        private List<CrackArea> cracks;
        public event Action<List<BMesh>,List<Bounds>,Bounds> OnNewCoreCreated = delegate { };
        public event Action<Bounds> DetochAllInRange = delegate { }; 

        public CrackModel(Vector2Int sizeOfTexture)
        {
            this.sizeOfTexture = sizeOfTexture;
            cracks = new List<CrackArea>();
        }

        public IEnumerable<(Vector2Int, Vector2Int)> GetPoints()
        {
            List<(Vector2Int, Vector2Int)> result = new List<(Vector2Int, Vector2Int)>();
            if (cracks is null || cracks.Count == 0)
                return result;
            foreach (var c in cracks)
            {
                if (c.hasStateChangedSinceLastPointsGet)
                    result.AddRange(c.GetPoints());
            }
            return result;
        }

        public IEnumerable<(Vector2Int, Vector2Int)> GetAllPoints()
        {
            List<(Vector2Int, Vector2Int)> result = new List<(Vector2Int, Vector2Int)>();
            if (cracks is null || cracks.Count == 0)
                return result;
            cracks.ForEach(c=>result.AddRange(c.GetPoints()));
            return result;
        }

        public IEnumerable<CrackCore> GetCores()
        {
            try
            {
                return cracks.Where(c => c is CrackCore).Cast<CrackCore>();
            }
            catch
            {
                return new List<CrackCore>();
            }
        }
    
        public bool AddCracks(Vector2 relativePosition, float force,float percentage)
        {
            var position = new Vector2Int((int)(relativePosition.x * sizeOfTexture.x),
                /*sizeOfTexture.y - */(int)(relativePosition.y * sizeOfTexture.y));
            var corePositions = GetCores().ToList();
            var isProlonged = false;
            foreach (var item in corePositions)
            {
                if (!(Vector2Int.Distance(item.position, position) < item.radius*1.4f))
                    continue;
                //cracks.AddRange(item.ProlongCrack(force));
                isProlonged = true;
                break;
            }
            Vector2 newPosition = new Vector2(MathExtensions.Rebase(relativePosition.x, 0, 1, -1, 1),
                MathExtensions.Rebase(relativePosition.y, 0, 1, -1, 1));
            Bounds rect = new Bounds
            {
                center = newPosition
            };

            if (isProlonged)
            {
                rect.size = Vector3.one * .15f * percentage;
                DetochAllInRange.Invoke(rect);
                return false;
            }

            CrackCore core = new CrackCore(position, CrackExtensions.TOKEN_DEFAULT_CORE_CIRCLE_RADIUS * percentage,
                out var generated, force);
            
            var res = CrackExtensions.SortVertices(core.ExitCrackPositions.Select(c => (Vector2)c), position);
            
            Vector2 center = Vector2.zero;
            
            for (int i = 0; i < res.Count; i++)
            {
                var item = res[i];
                //item.y = sizeOfTexture.y - item.y;
                res[i] = MathExtensions.Rebase(item, Vector2.zero, sizeOfTexture, -Vector2.one, Vector2.one);
                center += res[i];
                var newExitPoint = MathExtensions.Rebase(item, Vector2.zero, sizeOfTexture, -Vector2.one, Vector2.one);
                rect.Encapsulate(newExitPoint);
            }
            center /= res.Count;
            List<Bounds> bounds = new List<Bounds>();
            var bMesh = BMeshUtilities.CreateMeshesSegmentsFromPoints(res, center, Vector2.one * 10f, bounds);
            OnNewCoreCreated.Invoke(bMesh,bounds,rect);
            cracks.Add(core);
            cracks.AddRange(generated);
            return false;
        }
    }
}
