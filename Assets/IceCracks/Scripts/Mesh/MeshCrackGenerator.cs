using System.Collections.Generic;
using UnityEngine;

namespace IceCracks.CracksGeneration
{
    using Extensions;
    using Models;
    using Views;
    
    public class MeshCrackGenerator : MonoBehaviour
    {
        [SerializeField] private MeshCollider meshCollider;
        private MeshCrackVisualizer crackVisualizer;

        public Bounds bounds { get; private set; }
        private bool isInitialized;
        private Camera raycastCamera;

        private CrackModel model;
        public CrackModel Model => model;

        private bool isBusy => isAwaitingFor.Count != 0;

        private HashSet<object> isAwaitingFor = new HashSet<object>();


        public void Initialize(MeshCrackVisualizer meshCrackVisualizer, Camera raycastCamera)
        {
            if (isInitialized)
                return;
            this.raycastCamera = raycastCamera;
            crackVisualizer = meshCrackVisualizer;
            bounds = meshCollider.bounds;
            isInitialized = true;
            CreateData();
        }

        public void SetBusyStatus(object obj) => isAwaitingFor.Add(obj);

        public void UnsetBusyStatus(object obj) => isAwaitingFor.Remove(obj);

        private void CreateData()
        {
            model = new CrackModel(crackVisualizer.GetTextureSize());
        }

        private void OnMouseDown()
        {
            Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100))
            {
                if (bounds.Contains(hit.point))
                {
                    var relativePosition = GetRelativePositionOnMesh(hit.point);
                    crackVisualizer.IsDrawn = false;
                    bool isProlonged =
                        model.AddCracks(relativePosition, CrackExtensions.TOKEN_DEBUG_INITIAL_CRACK_FORCE);
                    crackVisualizer.DrawCracks(model.GetPoints(), GetPressPosition(relativePosition), isProlonged);
                }
            }
        }

        private Vector2Int GetPressPosition(Vector2 relativePos)
        {
            var textureSize = crackVisualizer.GetTextureSize();
            relativePos.x *= textureSize.x;
            relativePos.y *= textureSize.y;
            return new Vector2Int((int)relativePos.x, textureSize.y - (int)relativePos.y);
        }

        private Vector2 GetRelativePositionOnMesh(Vector3 initialPosition)
        {
            return new Vector2((initialPosition.x - bounds.min.x) / (bounds.max.x - bounds.min.x),
                (initialPosition.z - bounds.min.z) / (bounds.max.z - bounds.min.z));
        }

        /*
        private void FindAllClosedLoops(IEnumerable<(Vector2Int, Vector2Int)> lines)
        {
            HashSet<Vector2Int> points = new HashSet<Vector2Int>();
            Cycles c = new Cycles(); 
            foreach (var line in lines)
            {
                points.Add(line.Item1);
                points.Add(line.Item2);
            }
    
            var pointsList = points.ToList();
            c.V = pointsList;
            foreach (var line in  lines)
            {
                c.E.Add(new Edge(pointsList.IndexOf(line.Item1), pointsList.IndexOf(line.Item2)));
            }
            c.cyclesSearch();
            Debug.LogError("");
        }
    
        private class Edge
        {
            public int v1, v2;
    
            public Edge(int v1, int v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }
        }
        
    
        private class Cycles
        {
            public List<string> catalogCycles = new List<string>();
            public List<Vector2Int> V = new List<Vector2Int>();
            public List<Edge> E = new List<Edge>();
    
            public void cyclesSearch()
            {
                int[] color = new int[V.Count];
                for (int i = 0; i < V.Count; i++)
                {
                    for (int k = 0; k < V.Count; k++)
                        color[k] = 1;
                    List<int> cycle = new List<int>();
                    cycle.Add(i + 1);
                    DFScycle(i, i, E, color, -1, cycle);
                }
            }
    
            private void DFScycle(int u, int endV, List<Edge> E, int[] color, int unavailableEdge, List<int> cycle)
            {
                if (u != endV)
                    color[u] = 2;
                else if (cycle.Count >= 2)
                {
                    cycle.Reverse();
                    string s = cycle[0].ToString();
                    for (int i = 1; i < cycle.Count; i++)
                        s += "-" + cycle[i];
                    bool flag = false; 
                    for (int i = 0; i < catalogCycles.Count; i++)
                        if (catalogCycles[i] == s)
                        {
                            flag = true;
                            break;
                        }
    
                    if (!flag)
                    {
                        cycle.Reverse();
                        s = cycle[0].ToString();
                        for (int i = 1; i < cycle.Count; i++)
                            s += "-" + cycle[i];
                        catalogCycles.Add(s);
                    }
    
                    return;
                }
    
                for (int w = 0; w < E.Count; w++)
                {
                    if (w == unavailableEdge)
                        continue;
                    if (color[E[w].v2] == 1 && E[w].v1 == u)
                    {
                        List<int> cycleNEW = new List<int>(cycle);
                        cycleNEW.Add(E[w].v2 + 1);
                        DFScycle(E[w].v2, endV, E, color, w, cycleNEW);
                        color[E[w].v2] = 1;
                    }
                    else if (color[E[w].v1] == 1 && E[w].v2 == u)
                    {
                        List<int> cycleNEW = new List<int>(cycle);
                        cycleNEW.Add(E[w].v1 + 1);
                        DFScycle(E[w].v1, endV, E, color, w, cycleNEW);
                        color[E[w].v1] = 1;
                    }
                }
            }
        }
        */
    }
}