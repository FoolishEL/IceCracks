using System.Collections;
using UnityEngine;

namespace IceCracks.Interactions
{
    using Views;
    using CracksGeneration;
    public class SlowDissolve : MonoBehaviour
    {
        [SerializeField] private float dissolveTime = 7f;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private SimpleMeshView view;
        
        private void Reset()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            view = GetComponent<SimpleMeshView>();
        }

        private void Awake()
        {
            SplitMeshController.OnRestartCommandRecive += ForceDissolveImmediate;
        }
        
        private void OnDestroy()
        {
            SplitMeshController.OnRestartCommandRecive -= ForceDissolveImmediate;
        }

        private void ForceDissolveImmediate()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private float currentTime;
        private Coroutine coroutine;

        private void OnEnable()
        {
            dissolveTime += Random.Range(-3f, 7f);
            currentTime = 0f;
            coroutine = StartCoroutine(Dissolve());
        }

        private void OnDisable()
        {
            StopCoroutine(coroutine);
        }


        private IEnumerator Dissolve()
        {
            while (currentTime<dissolveTime)
            {
                yield return null;
                currentTime += Time.deltaTime;
                meshRenderer.materials[0].color = Color.Lerp(Color.white, Color.clear, currentTime / dissolveTime);
                transform.GetChild(0).localScale = Vector3.Lerp(Vector3.one, Vector3.zero, currentTime / dissolveTime);
            }
            CrackPiecePool.Instance.ReleaseView(view);
        }
    }
}
