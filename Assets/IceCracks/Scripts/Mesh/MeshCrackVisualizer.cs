using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ProtoTurtle.BitmapDrawing;

public class MeshCrackVisualizer : MonoBehaviour
{
    [SerializeField] private Material materialInitial;
    private Material currentMaterial;

    [SerializeField] private Color color;
    [SerializeField] private Texture2D texture2D;
    [SerializeField] private MeshRenderer meshRenderer;

    private bool _isInited = false;

    public void Initialize()
    {
        if (_isInited)
            return;
        _isInited = true;
        currentMaterial = Instantiate(materialInitial);
        Texture2D copyTexture = new Texture2D(texture2D.width, texture2D.height);
        copyTexture.SetPixels(texture2D.GetPixels());
        copyTexture.Apply();
        texture2D = copyTexture;
        currentMaterial.mainTexture = texture2D;
        meshRenderer.materials = new[] { currentMaterial };
    }

    public async Task DrawCracks(IEnumerable<(Vector2Int, Vector2Int)> lines, Vector2Int center)
    {
        //TODO: refactor this
        lines = lines.OrderBy(c => Vector2Int.Distance(center, c.Item1));
        var first = lines.Where((item, index) => index % 3 == 0);
        var second = lines.Where((item, index) => index % 3 == 1);
        var third = lines.Where((item, index) => index % 3 == 2);
        await Task.WhenAll(RawDraw(first), RawDraw(second), RawDraw(third));
    }

    private async Task RawDraw(IEnumerable<(Vector2Int, Vector2Int)> lines)
    {
        foreach (var line in lines)
        {
            texture2D.DrawLine(line.Item1, line.Item2, color);
            texture2D.Apply();
            await Task.Yield();
        }
    }

    public void DebugDrawPoint(Vector2Int position)
    {
        texture2D.DrawFilledCircle(position.x, position.y, 5, color);
        texture2D.Apply();
    }

    public Vector2Int GetTextureSize()
    {
        return new Vector2Int(texture2D.width, texture2D.height);
    }
}