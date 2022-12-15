using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pet.Slime
{
	public class SwimmingObjectsController : MonoBehaviour
	{
		public static SwimmingObjectsController Instance;
		[Header("Is2d")]
		public bool selectedIs2d;

		[Header("3d")]
		public GameObject[] itemsPrefabs;
		public string resourceSpritesPath_objects;
		public string[] objectsNames;

		public Transform parent3dItems;
		public GameObject selectedbjectPrefab;

		[Header("3d Sprites For User Slimes")]
		public Sprite[] sprites3d;
		public string resourceSpritesPath_sprites3d;
		public string[] spritesNames3d;

		[Header("2d")]
		public GameObject items2dPrefab;
		public Transform parent2dItems;
		public Sprite[] sprites2d;
		public string resourceSpritesPath_sprites2d;
		public string resourceSpritesPath_sprites2d_VIP;
		public string[] spritesNames2d;

		public Sprite selectedSprite;

		[Header("Coin")]
		public GameObject coinPrefab;
		public Transform coinsParent;
		public AudioSource coinSound;

		[Header("Crystal")]
		public GameObject crystalPrefab;
		public Transform crystalsParent;

		[Header("Vertices")]
		public Vector3[] clothVertices;
		//public Vector3[] normals;

		[Header("RandomColors")]
		public Color[] randomColors;
		int tempInt;

		private List<GameObject> spawnedItems = new List<GameObject>();

		public GameObject CreatNewObject_3d(bool inJar = false) {
			//		int rnd = Random.Range (0, MeshDeformer.Instance.displacedVertices.Length);
			int rnd = UnityEngine.Random.Range(0, Mathf.Min(SlimeMeshDeformer.Instance.posNew.Length, clothVertices.Length));

			if (inJar) {
				rnd = UnityEngine.Random.Range(20, 30) * 50 + 25 + UnityEngine.Random.Range(-step, step);
				//Debug.Log("InJar");
			}

			GameObject go = (GameObject) Instantiate(selectedbjectPrefab, SlimeMeshDeformer.Instance.posNew[rnd], Quaternion.identity, parent3dItems);
			go.transform.rotation = Quaternion.Euler(UnityEngine.Random.insideUnitSphere * 360f);
			go.GetComponent<SwimmingObject>().numberOfVertex = rnd;

			if (inJar) {
				go.name += "_InJar";
			}

			//		Color clr = go.GetComponent<MeshRenderer>().material.color;
			//		clr.a = Random.Range (0.6f, 1f);
			//		go.GetComponent<MeshRenderer> ().material.color = clr;

			return go;
		}

		public GameObject CreatNewObject_2d() {
			//		int rnd = Random.Range (0, MeshDeformer.Instance.displacedVertices.Length);
			int rnd = UnityEngine.Random.Range(0, Mathf.Min(SlimeMeshDeformer.Instance.posNew.Length, clothVertices.Length));

			GameObject go = (GameObject) Instantiate(items2dPrefab, SlimeMeshDeformer.Instance.posNew[rnd], Quaternion.Euler(-90, 180, 0), parent2dItems);

			go.GetComponent<SpriteRenderer>().sprite = selectedSprite;

			go.GetComponent<SwimmingObject>().numberOfVertex = rnd;

			//if (selectedSprite.name == "Custom_93") { //hardcode
			//	SpriteRendererSpriteChanger spr = go.AddComponent<SpriteRendererSpriteChanger> ();
			//	spr.sprites = new Sprite[3];
			//	spr.sprites [0] = Get2dSprite (92);
			//	spr.sprites [1] = Get2dSprite (93);
			//	spr.sprites [2] = Get2dSprite (94);
			//}

			Color clr = Color.white;

			if (Int32.TryParse(selectedSprite.name, out tempInt)) {
				if (IsNewMiniItems(tempInt)) { //За кристаллы которые и в make новые
											   //			if ((tempInt >= 110) && (tempInt < 130)) { //За кристаллы которые и в make новые
											   //				SpriteRendererColorChanger srcc = go.AddComponent<SpriteRendererColorChanger> ();
					clr = randomColors[UnityEngine.Random.Range(0, randomColors.Length)];
					//go.GetComponent<SpriteRenderer> ().color = clr;
					go.GetComponent<SpriteRenderer>().size = new Vector2(2.6f, 2.6f);
				}
				if ((tempInt >= 200) && (tempInt <= 218)) {
					go.GetComponent<SpriteRenderer>().size = new Vector2(3.3f, 3.3f);
				} else if (tempInt == 219) {
					go.GetComponent<SpriteRenderer>().size = new Vector2(10f, 10f);
				} else if (tempInt == 220) {
					go.GetComponent<SpriteRenderer>().size = new Vector2(7f, 7f);
				} else {
					go.GetComponent<SpriteRenderer>().size = new Vector2(5f, 5f);
				}

				//Debug.Log(tempInt);
			} else {
				go.GetComponent<SpriteRenderer>().size = new Vector2(4f, 4f);
			}

			clr.a = UnityEngine.Random.Range(0.5f, 1f);
			go.GetComponent<SpriteRenderer>().color = clr;
			go.GetComponent<SpriteRenderer>().sortingOrder = (int) (clr.a * 100);

			return go;
		}

		void Awake() {
			Instance = this;
		}

		public void UpdateClothVertices() {
			if (SlimeMeshDeformer.Instance) {
				clothVertices = SlimeMeshDeformer.Instance.posNew;
				//normals = SlimeMeshDeformer.Instance.normals;
			}
		}

		void Update() {
			if (SlimeMeshDeformer.Instance) {
				clothVertices = SlimeMeshDeformer.Instance.posNew;
				//normals = SlimeMeshDeformer.Instance.normals;
			}
		}

		public void SetItem(int num, bool _is2d, bool _isVip = false) {
			isVip = _isVip;
			selectedIs2d = _is2d;

			if (selectedIs2d) {
				selectedSprite = Get2dSprite(num);
			} else {
				selectedbjectPrefab = Get3DPrefab(num);
			}

		}

		public void ClearObjects() {
			if (spawnedItems.Count > 0) {
				foreach (GameObject go in spawnedItems) {
					Destroy(go);
				}
				spawnedItems.Clear();
			}

			InitItemsInJar();
		}

		public void CreateObjects(int count = 1) {
			StartCoroutine(CreateObjects_Cor(count));
		}

		public int items3DInJar = 5;
		public int step = 50;

		public void InitItemsInJar() {
			itemsToSpawnInJar = UnityEngine.Random.Range(1, items3DInJar + 1);
			itemsSpawnedInJar = 0;

			//Debug.Log("InitItemsInJar: " + itemsToSpawnInJar);
		}

		public int itemsToSpawnInJar = 0;
		public int itemsSpawnedInJar = 0;

		IEnumerator CreateObjects_Cor(int count = 1) {
			yield return new WaitForEndOfFrame();

			if (selectedIs2d) {
				for (int i = 0; i < count; i++) {
					spawnedItems.Add(CreatNewObject_2d());
				}
			} else {
				if (selectedbjectPrefab != null) {
					//int tmp = items3DInJar = UnityEngine.Random.Range(1, items3DInJar + 1);
					//Debug.Log("items3DInJar: " + tmp);

					for (int i = 0; i < count; i++) {
						spawnedItems.Add(CreatNewObject_3d((itemsSpawnedInJar < itemsToSpawnInJar) ? true : false));

						if (itemsSpawnedInJar < itemsToSpawnInJar) {
							itemsSpawnedInJar++;

							//Debug.Log("itemsSpawnedInJar: " + itemsSpawnedInJar);
						}
					}
				}
			}

		}

		public int GetCountItems() {
			return parent3dItems.childCount + parent2dItems.childCount;
		}

		#region Coin
		public void GenerateCoin() {
			GameObject go = CreatNewCoin();
			Destroy(go, SlimeGame.Instance.timeDestroyCoin);
		}

		public GameObject CreatNewCoin() {
			int rnd = 0;
			do {
				rnd = UnityEngine.Random.Range(400, SlimeMeshDeformer.Instance.posNew.Length - 400);
			} while (SlimeMeshDeformer.Instance.vecData[rnd].isPermanentlyFixed);

			GameObject go = (GameObject) Instantiate(coinPrefab, SlimeMeshDeformer.Instance.posNew[rnd], Quaternion.identity, coinsParent);
			go.transform.rotation = Quaternion.Euler(45f, UnityEngine.Random.Range(0, 360f), 0f);
			go.GetComponent<SwimmingObject>().numberOfVertex = rnd;

			return go;
		}
		#endregion

		//#region Crystal
		//public void GenerateCrystal() {
		//	GameObject go = CreatNewCrystal();
		//	Destroy(go, SlimeGame.Instance.timeDestroyCrystal);
		//}

		//public GameObject CreatNewCrystal() {
		//	int rnd = 0;
		//	do {
		//		rnd = UnityEngine.Random.Range(400, SlimeMeshDeformer.Instance.posNew.Length - 400);
		//	} while (SlimeMeshDeformer.Instance.vecData[rnd].isPermanentlyFixed);

		//	GameObject go = (GameObject) Instantiate(crystalPrefab, SlimeMeshDeformer.Instance.posNew[rnd], Quaternion.identity, crystalsParent);
		//	go.transform.rotation = Quaternion.Euler(45f, UnityEngine.Random.Range(0, 360f), 0f);
		//	go.GetComponent<SwimmingObject>().numberOfVertex = rnd;

		//	return go;
		//}
		//#endregion



		//	#region ForHelix
		//	public GameObject Get2DPrefab(){
		//			return items2dPrefab;
		//	}

		public Sprite Get2dSprite(int num, bool _isVip = false) {
			//Debug.Log(num);
			//if (num < 0) {
			//	num = 100 - num;
			//}

			string spritePath = resourceSpritesPath_sprites2d + spritesNames2d[num];

			//if (isVip || _isVip) {
			//	spritePath = resourceSpritesPath_sprites2d_VIP + num;
			//}
			return Resources.Load<Sprite>(spritePath);
			//		return sprites2d [num];
		}
		public bool isVip = false;
		public GameObject Get3DPrefab(int num) {
			return Resources.Load<GameObject>(resourceSpritesPath_objects + objectsNames[num]);
			//		return itemsPrefabs [num];
		}

		//	#endregion

		public Sprite Get3dSprite(int num) {
			return Resources.Load<Sprite>(resourceSpritesPath_sprites3d + spritesNames3d[num]);
			//return ResourcesLoader.LoadSprite (resourceSpritesPath_sprites3d + spritesNames3d[num]);
			//		return sprites3d [num];
		}

		public bool IsNewMiniItems(int index, bool isPPindex = false) {
			if (isPPindex) {
				if ((index <= -10) && (index > -30)) {
					return true;
				} else {
					return false;
				}
			} else {
				if ((tempInt >= 110) && (tempInt < 130)) {
					return true;
				} else {
					return false;
				}
			}
			//		return false;
		}



		[ContextMenu("Set_SpritesNames_2dAnd3d")]
		public void Set_SpritesNames_2dAnd3d() {
			spritesNames2d = new string[sprites2d.Length];
			for (int i = 0; i < sprites2d.Length; i++) {
				if (sprites2d[i] != null) {
					spritesNames2d[i] = sprites2d[i].name;
				}
			}

			spritesNames3d = new string[sprites3d.Length];
			for (int i = 0; i < sprites3d.Length; i++) {
				if (sprites3d[i] != null) {
					spritesNames3d[i] = sprites3d[i].name;
				}
			}
		}

		[ContextMenu("Set_DeleteSprites")]
		public void Set_DeleteSprites_S2_gradients() {
			sprites2d = new Sprite[0];
			sprites3d = new Sprite[0];
		}

		[ContextMenu("Set_GameObjectsName")]
		public void Set_GameObjectsName() {
			objectsNames = new string[itemsPrefabs.Length];

			for (int i = 0; i < itemsPrefabs.Length; i++) {
				if (itemsPrefabs[i] != null) {
					objectsNames[i] = itemsPrefabs[i].name;
				}
			}

		}

		[ContextMenu("Set_DeletePrefabs")]
		public void Set_DeletePrefabs() {
			itemsPrefabs = new GameObject[0];
		}

		[ContextMenu("CheckAll")]
		public void CheckAll() { // Пытается загрузить все спрайты, если не получится, то будет писать ошибки
			for (int i = 0; i < spritesNames2d.Length; i++) {
				Resources.Load<Sprite>(resourceSpritesPath_sprites2d + spritesNames2d[i]);
			}

			for (int i = 0; i < spritesNames3d.Length; i++) {
				Resources.Load<Sprite>(resourceSpritesPath_sprites3d + spritesNames3d[i]);
			}

			for (int i = 0; i < objectsNames.Length; i++) {
				Resources.Load<GameObject>(resourceSpritesPath_objects + objectsNames[i]);
			}
		}
	}
}