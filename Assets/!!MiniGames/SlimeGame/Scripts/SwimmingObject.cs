using UnityEngine;

namespace Pet.Slime{
	public class SwimmingObject : MonoBehaviour {

		public int numberOfVertex = -1; // Номер Вершины к которой привязан объект

		SwimmingObjectsController swimmingObjectsController;

		Vector3 randomRotate;
		[Header ("Rotation")]
		public bool isNeedRotate = true;
		public float timeChangeRotate;
		float timerChangeRotate;
		public float speedRotate;

		public Vector3 customRotate;

		[Header ("Sprite Renderer For 2d")]
		public SpriteRenderer spriteRenderer;

		public bool isNeedStartRotateY;

		void Start(){
			swimmingObjectsController = SwimmingObjectsController.Instance;
			timeChangeRotate += Random.Range (-0.4f , 0.4f);

			if (isNeedStartRotateY) {
				transform.Rotate (Vector3.up, Random.Range (0f, 360f), Space.World);
			}
		}

		void LateUpdate () {
			if (isNeedRotate) {
				if (customRotate != Vector3.zero) {
	//				transform.eulerAngles += customRotate * Time.deltaTime;
					transform.Rotate (Vector3.up,  customRotate.y * Time.deltaTime, Space.Self);
				} else {
					transform.eulerAngles += randomRotate;

					timerChangeRotate -= Time.deltaTime;
					if (timerChangeRotate <= 0) {
						timerChangeRotate = timeChangeRotate;
						randomRotate = Random.onUnitSphere * speedRotate;
					}
				}

			}

			if (numberOfVertex != -1) {
				transform.localPosition = swimmingObjectsController.clothVertices[numberOfVertex];

				//if (swimmingObjectsController.normals != null) {
				//    transform.LookAt(transform.position + swimmingObjectsController.normals[numberOfVertex]);
				//    transform.Rotate(Vector3.right, -90 , Space.Self);
				//}

			}
		}
	}
}