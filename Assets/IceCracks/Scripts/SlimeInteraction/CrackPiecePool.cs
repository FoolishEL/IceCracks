using UnityEngine;
using UnityEngine.Pool;

namespace IceCracks.CracksGeneration
{
    using Views;
    public class CrackPiecePool : MonoBehaviour
    {
        [SerializeField] private SimpleMeshView prefab;
        [SerializeField] private int maxSize = 120;
        public static CrackPiecePool Instance { get; private set; }

        private ObjectPool<SimpleMeshView> objectPool;

        private void Awake()
        {
            if (Instance is null)
            {
                Instance = this;
                PreparePool();
            }
            else
                Destroy(this);
        }
        
        private void OnDestroy() => Instance = null;

        private void PreparePool()
        {
            objectPool = new ObjectPool<SimpleMeshView>
            (
                () =>
                {
                    var instance = Instantiate(prefab, transform, true);
                    instance.transform.position = transform.position;
                    return instance;
                },
                (c) =>
                {
                    c.gameObject.SetActive(true);
                    c.transform.position = transform.position;
                    c.transform.rotation = Quaternion.identity;
                    // c.transform.GetChild(0).localEulerAngles =
                    //     new Vector3(Random.Range(-35f, 35f), Random.Range(-35f, 35f), 0);
                },
                (c) => c.gameObject.SetActive(false),
                null, true, 10, maxSize
            );
        }

        public SimpleMeshView GetView() => objectPool.Get();

        public void ReleaseView(SimpleMeshView view) => objectPool.Release(view);
    }
}