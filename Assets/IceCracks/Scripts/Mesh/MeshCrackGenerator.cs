using System.Collections.Generic;
using System.Linq;
using IceCracks.CracksGeneration.Extensions;
using IceCracks.CracksGeneration.Models;
using UnityEngine;

public class MeshCrackGenerator : MonoBehaviour
{
    [SerializeField] private MeshCollider meshCollider;
    [SerializeField] private MeshCrackVisualizer crackVisualizer;

    private Bounds bounds;
    private bool isInitialized;

    private CrackModel model;

    private void Start()
    {
        bounds = meshCollider.bounds;
        Initialize();
        crackVisualizer.Initialize();
    }

    public void Initialize()
    {
        if (isInitialized)
            return;
        isInitialized = true;
        CreateData();
    }

    private void CreateData()
    {
        model = new CrackModel(crackVisualizer.GetTextureSize());
    }

    private void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 100))
        {
            var position = hit.point;
            if (bounds.Contains(position))
            {
                var pos = GetPressPosition(hit.point);
                model.AddCracks(pos, CrackExtensions.TOKEN_DEBUG_INITIAL_CRACK_FORCE);
                crackVisualizer.DrawCracks(model.GetPoints(), pos);
                //FindAllClosedLoops(model.GetAllPoints());
            }
        }
    }

    private Vector2Int GetPressPosition(Vector3 initialPosition)
    {
        var textureSize = crackVisualizer.GetTextureSize();
        var boundSize = new Vector2(bounds.size.x, bounds.size.z);
        var center = textureSize / 2;
        var pos = bounds.center - initialPosition;
        pos.x *= -textureSize.x / boundSize.x;
        pos.z *= textureSize.y / boundSize.y;
        center += new Vector2Int((int)pos.x, (int)pos.z);
        return center;
    }

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
                //поскольку в C# нумерация элементов начинается с нуля, то для
                //удобочитаемости результатов поиска в список добавляем номер i + 1
                cycle.Add(i + 1);
                DFScycle(i, i, E, color, -1, cycle);
            }
        }

        private void DFScycle(int u, int endV, List<Edge> E, int[] color, int unavailableEdge, List<int> cycle)
        {
            //если u == endV, то эту вершину перекрашивать не нужно, иначе мы в нее не вернемся, а вернуться необходимо
            if (u != endV)
                color[u] = 2;
            else if (cycle.Count >= 2)
            {
                cycle.Reverse();
                string s = cycle[0].ToString();
                for (int i = 1; i < cycle.Count; i++)
                    s += "-" + cycle[i].ToString();
                bool flag = false; //есть ли палиндром для этого цикла графа в List<string> catalogCycles?
                for (int i = 0; i < catalogCycles.Count; i++)
                    if (catalogCycles[i].ToString() == s)
                    {
                        flag = true;
                        break;
                    }

                if (!flag)
                {
                    cycle.Reverse();
                    s = cycle[0].ToString();
                    for (int i = 1; i < cycle.Count; i++)
                        s += "-" + cycle[i].ToString();
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
}