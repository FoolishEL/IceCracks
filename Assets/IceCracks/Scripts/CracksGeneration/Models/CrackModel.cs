using System;
using System.Collections.Generic;
using System.Linq;
using IceCracks.Math;
using UnityEngine;
using static BMeshUtilities;

namespace IceCracks.CracksGeneration.Models
{
    using Extensions;

    public class CrackModel
    {
        private Vector2Int sizeOfTexture;
        private List<CrackArea> cracks;
        public event Action<Bounds> OnNewCoreCreated = delegate { };

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
    
        public void AddCracks(Vector2 relativePosition, float force)
        {
            var position = new Vector2Int((int)(relativePosition.x * sizeOfTexture.x),
                sizeOfTexture.y - (int)(relativePosition.y * sizeOfTexture.y));
            var corePositions = GetCores().ToList();
            var isProlonged = false;
            foreach (var item in corePositions)
            {
                if (!(Vector2Int.Distance(item.position, position) < item.radius * 1.2f)) continue;
                cracks.AddRange(item.ProlongCrack(force));
                isProlonged = true;
                break;
            }

            if (isProlonged) return;
            CrackCore core = new CrackCore(position, CrackExtensions.TOKEN_DEFAULT_CORE_CIRCLE_RADIUS, out var generated, force);
            Vector2 tlOffset = (Vector2.left + Vector2.up).normalized * (core.radius / (sizeOfTexture.magnitude * 2));
            Vector2 brOffset = (Vector2.right + Vector2.down).normalized *
                               (core.radius / (sizeOfTexture.magnitude * 2));
            Vector2 newPosition = new Vector2(MathExtensions.Rebase(relativePosition.x, 0, 1, -1, 1),
                MathExtensions.Rebase(relativePosition.y, 0, 1, -1, 1));
            Bounds rect = new Bounds(newPosition, Vector2.one * (core.radius / (sizeOfTexture.magnitude * 2)));
            OnNewCoreCreated.Invoke(rect);
            cracks.Add(core);
            cracks.AddRange(generated);
        }
    }
}
