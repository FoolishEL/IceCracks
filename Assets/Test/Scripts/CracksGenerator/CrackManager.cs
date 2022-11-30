using UnityEngine;

public class CrackManager : MonoBehaviour
{
    [SerializeField] private CrackGenerator crackGenerator;
    [SerializeField] private CrackVisualizer crackVisualizer;

    private void Start()
    {
        crackVisualizer.Initialize();
        crackGenerator.Initialize(crackVisualizer);
    }
    
    [ContextMenu(nameof(Restart))]
    private void Restart()
    {
        crackVisualizer.Restart();
        crackGenerator.Restart();
    }
}
