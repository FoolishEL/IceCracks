using IceCracks.Math;
using Pet.Slime;
using UnityEngine;

namespace IceCracks.Interactions
{
    public class IceCrackSwimming : MonoBehaviour
    {
        [SerializeField] private Transform transformToRotate;
        public int numberOfVertex = -1; // Номер Вершины к которой привязан объект

        SwimmingObjectsController swimmingObjectsController;

        private const float MAX_SISZE = 0.34f;
        private const float minSpeed = 3f, maxSpeed = 10f;
        
        
        public Vector3 offset;
        private Quaternion rotation;

        public float size = -1f;
        private float speed;

        void Start()
        {
            rotation = Quaternion.Euler(90, 0, 0);
            swimmingObjectsController = SwimmingObjectsController.Instance;
            speed = Mathf.Lerp(maxSpeed, minSpeed, size / MAX_SISZE);
            if (MathExtensions.GetRandomWithPercent(.5f))
            {
                speed *= -1;
            }
            speed += Random.Range(-2, 2f);
        }

        void LateUpdate()
        {
            // if (size > 0f)
            //     transform.localRotation *= Quaternion.Euler(0, speed * Time.deltaTime, 0f);
            if (numberOfVertex != -1)
            {
                transform.localPosition =
                    rotation * (swimmingObjectsController.clothVertices[numberOfVertex] + offset);
            }
        }
    }
}