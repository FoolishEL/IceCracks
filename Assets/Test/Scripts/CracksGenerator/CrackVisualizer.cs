using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProtoTurtle.BitmapDrawing;

public class CrackVisualizer : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;

    [SerializeField] private Color color;

    private Texture2D texture2D;
    private RectTransform rectTransform;


    private bool _isInited = false;

    public void Initialize()
    {
        if(_isInited)
            return;
        _isInited = true;
        
        rectTransform = transform as RectTransform;
        texture2D = new Texture2D((int)rectTransform.sizeDelta.x, (int)rectTransform.sizeDelta.y);
        rawImage.texture = texture2D;
    }
    
    public void DrawCracks(IEnumerable<(Vector2Int,Vector2Int)> lines)
    {
        foreach (var line in lines)
        {
            texture2D.DrawLine(line.Item1, line.Item2, color);
        }
        texture2D.Apply();
        rawImage.texture = texture2D;
    }
    
    public void DebugDrawPoint(Vector2Int position)
    {
        texture2D.DrawFilledCircle(position.x, position.y, 5, color);
        texture2D.Apply();
    }

    public Vector2Int GetTextureSize()
    {
        if (rectTransform is not null)
        {
            return new Vector2Int((int)rectTransform.sizeDelta.x, (int)rectTransform.sizeDelta.y);
        }
        return Vector2Int.zero;
    }
}