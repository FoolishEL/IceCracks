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
            Debug.Log($"{position}");
            if (bounds.Contains(position))
            {
                var pos = GetPressPosition(hit.point);
                model.AddCracks(pos, CrackExtensions.TOKEN_DEBUG_INITIAL_CRACK_FORCE);
                crackVisualizer.DrawCracks(model.GetPoints(), pos);
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
}

