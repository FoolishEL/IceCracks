using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IceCracks.CracksGeneration.Models
{
    using Extensions;

    public class CrackModel
    {
        private Vector2 sizeOfTexture;
        private List<CrackArea> cracks;

        public CrackModel(Vector2 sizeOfTexture)
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
    
        public void AddCracks(Vector2Int position, float force)
        {
            var corePositions = GetCores().ToList();
            bool isCreated = false;
            foreach (var item in corePositions)
            {
                if (Vector2Int.Distance(item.position, position) < item.radius)
                {
                    cracks.AddRange(item.ProlongCrack(force));
                    isCreated = true;
                    break;
                }
            }
            if (!isCreated)
            {
                CrackCore core = new CrackCore(position, CrackExtensions.TOKEN_DEFAULT_CORE_CIRCLE_RADIUS, out var generated, force);
                cracks.Add(core);
                cracks.AddRange(generated);
            }
        }
    }
}
