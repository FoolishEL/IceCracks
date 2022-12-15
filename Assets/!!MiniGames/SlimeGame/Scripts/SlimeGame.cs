using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pet.Slime
{
	public class SlimeGame : MonoBehaviour
	{

		public static SlimeGame Instance;

		[Header("General")]
		public bool isStarted;
		public bool isTouch;

		public GameObject slimeUI;
		public GameObject slimeContainer;

		public SwimmingObjectsController soc;
		public SlimeMeshDeformer SMD;

		public Renderer slimeRenderer;

		[Header("Slime Data")]
		public SlimeData_List slimesData;

		public AudioClip[] slimeSounds;
		public AudioClip currentSlimeSound;

		[Header("Slime Sound Settings")]
		public AudioSource slimeSoundSource;
		public bool isTouchAndMove;
		public float timerCheckSlimeSound = 0.2f;

		private Vector3 oldMousePos;
		private Vector3 mousePos;
		private float timeCheckSlimeSound;

		[Header("Bonus Multiplier")]
		public bool isBonusActivate;
		public float bonusMultiplierMoney = 2f;

		public float timer_Bonus;
		public float timeBonus = 120f;

		public GameObject bonusIcon;
		public UI2DSprite bonusBar;

		[Header("Coin")]
		public float baseTimeSpawnCoin = 5f;
		public float timeRandomCoin = 2f;
		public float timeDestroyCoin = 5f;
		public float timer_SpawnCoin = 0f;
		public bool X2_Spawn_Coin = false;
		public float X2_Spawn_Coin_time = 120f;
		private float X2_Spawn_Coin_timer;
		public GameObject X2_Spawn_Coin_Icon;
		public UI2DSprite X2_Spawn_Coin_Progress;

		public BonusStar x2coinScript;

		[Header("Settings")]
		public BonusStar[] bonuses;

		
		public bool pickRandomSlimeOnStart = false;
		public bool alwaysUseDefaultSlime = false;


		void Awake() {
			Instance = this;

			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			ApplyDefaultSettings();

			//Debug.Log("Vibration enabled: " + VibrationManager.VibrationEnabled);
			//Debug.Log("Shadows enabled: " + ShadowsManager.ShadowsEnabled);
		}

		void Start() {
			if(pickRandomSlimeOnStart){
				OpenSlime(Random.Range(0, slimesData.list.Count));
			} else {
				OpenSlime(0);
			}
			
		}

		void ApplySlimeSettings() {
			Physics2D.gravity = new Vector2(0, -7);
			Physics2D.velocityIterations = 1;
			Physics2D.positionIterations = 1;
			Physics2D.maxLinearCorrection = 1;
			Physics2D.maxAngularCorrection = 4;
			Physics2D.maxRotationSpeed = 180;
			Physics2D.timeToSleep = 0.2f;
			Physics2D.queriesHitTriggers = false;
			Physics2D.queriesStartInColliders = false;
			Physics2D.callbacksOnDisable = false;

			Time.fixedDeltaTime = 0.02f;
			Time.maximumDeltaTime = 0.05f;

			Application.targetFrameRate = 300; //10000
		}


		void ApplyDefaultSettings() {
			Physics2D.gravity = new Vector2(0, -9.81f);
			Physics2D.velocityIterations = 10;
			Physics2D.positionIterations = 5;
			Physics2D.maxLinearCorrection = 0.2f;
			Physics2D.maxAngularCorrection = 8;
			Physics2D.maxRotationSpeed = 360;
			Physics2D.timeToSleep = 0.2f;
			Physics2D.queriesHitTriggers = true;
			Physics2D.queriesStartInColliders = false;
			Physics2D.callbacksOnDisable = true;

			Time.fixedDeltaTime = 0.01f;
			Time.maximumDeltaTime = 0.1f;

			Application.targetFrameRate = 60; //10000
		}

		void Update() {
			if (isStarted) {
				if (!X2_Spawn_Coin) {
					timer_SpawnCoin -= Time.deltaTime;
				} else {
					timer_SpawnCoin -= Time.deltaTime * 2f;

					X2_Spawn_Coin_timer -= Time.deltaTime;

					if (X2_Spawn_Coin_Progress != null) {
						X2_Spawn_Coin_Progress.fillAmount = X2_Spawn_Coin_timer / X2_Spawn_Coin_time;
					}

					if (X2_Spawn_Coin_timer <= 0) {
						X2_Spawn_Coin = false;

						if (X2_Spawn_Coin_Icon != null) {
							X2_Spawn_Coin_Icon.SetActive(false);
						}
						PlayerPrefs.SetFloat("BroGame_X2_Spawn_Coin_timer", 0f);
					}

					if (Time.frameCount % 100 == 0) {
						PlayerPrefs.SetFloat("BroGame_X2_Spawn_Coin_timer", X2_Spawn_Coin_timer);
					}
				}

				if (timer_SpawnCoin <= 0) {
					if (soc != null) {
						soc.GenerateCoin();
					}
					timer_SpawnCoin = baseTimeSpawnCoin + Random.Range(-timeRandomCoin, timeRandomCoin);
				}

				//if (timer_SpawnCrystal <= 0) {
				//	if (soc != null) {
				//		soc.GenerateCrystal();
				//	}
				//	timer_SpawnCrystal = baseTimeSpawnCrystal + Random.Range(-timeRandomCrystal, timeRandomCrystal);
				//}
			}

			if (isBonusActivate) {
				timer_Bonus -= Time.deltaTime;

				if (bonusBar != null) {
					bonusBar.fillAmount = timer_Bonus / timeBonus;
				}
				if (timer_Bonus <= 0) {
					isBonusActivate = false;
					if (bonusIcon != null) {
						bonusIcon.SetActive(false);
					}
					PlayerPrefs.SetFloat("BroGame_Timer_Bonus", 0f);

					x2coinScript.BonusSetActive(true);

					//if (bonuses.Length > 0 && bonuses[0] != null) {
					//	bonuses[0].BonusSetActive(true);
					//}
				}

				if (Time.frameCount % 100 == 0) {
					PlayerPrefs.SetFloat("BroGame_Timer_Bonus", timer_Bonus);
				}
			}
		}

		void LateUpdate() {
			if (slimeContainer == null) {
				return;
			}

			if ((!isStarted) && (!slimeContainer.activeSelf)) {
				return;
			}

			try {
				if (SMD.IsMeshTouched() && !isTouch) {
					isTouch = true;

					//if (SoundPlayer.SoundEnabled) {
					//	slimeSoundSource.Play();
					//	slimeSoundSource.Pause();
					//}

					slimeSoundSource.Play();
					slimeSoundSource.Pause();

					timeCheckSlimeSound = 0f;
				} else if (!SMD.IsMeshTouched() && isTouch) {
					isTouch = false;

					//if (SoundPlayer.SoundEnabled) {
					//	slimeSoundSource.Stop();
					//}

					slimeSoundSource.Stop();

					isTouchAndMove = false;
				}

				if (SMD.IsMeshTouched()) {
					mousePos = Input.mousePosition;

					timeCheckSlimeSound -= Time.deltaTime;

					if (timeCheckSlimeSound <= 0f) {
						if (mousePos != oldMousePos) {
							if (!slimeSoundSource.isPlaying) {
								//if (SoundPlayer.SoundEnabled) {
								//	slimeSoundSource.UnPause();
								//}

								slimeSoundSource.UnPause();
								isTouchAndMove = true;
							}
						} else {
							//if (SoundPlayer.SoundEnabled) {
							//	slimeSoundSource.Pause();
							//}

							slimeSoundSource.Pause();
							isTouchAndMove = false;
						}
					}

					oldMousePos = mousePos;
				}
			} catch (System.Exception ex) {
				Debug.Log(ex.Message);
			}
		}

		#region Bonus on Slime
		public void CheckAllBonuses() {
			for (int i = 0; i < bonuses.Length; i++) {
				bonuses[i].Check();
			}
		}

		public void ActivateBonus(bool resetTimer = true) {
			isBonusActivate = true;
			bonusIcon.SetActive(true);
			x2coinScript.BonusSetActive(false);
			if (resetTimer) {
				timer_Bonus = timeBonus;
			}
		}

		public void Activate_X2_Coin(bool resetTimer = true) {
			X2_Spawn_Coin = true;
			X2_Spawn_Coin_Icon.SetActive(true);
			if (resetTimer) {
				X2_Spawn_Coin_timer = X2_Spawn_Coin_time;
			}
		}
		#endregion

		public void ChangeSlimeSound(int num) {
			currentSlimeSound = slimeSounds[num];
			slimeSoundSource.clip = currentSlimeSound;
		}

		public void OpenSlime(int index) {
			ApplySlimeSettings();

			slimeUI.SetActive(true);
			slimeContainer.SetActive(true);
			Camera.main.backgroundColor = Color.white;
			SMD.Initialize();
			soc.UpdateClothVertices();

			LoadGame(slimesData.list[index]);

			isStarted = true;

			Time.timeScale = 1;
		}

		void LoadGame(SlimeData slimeData) {
			ChangeSlimeSound(slimeData.slimeSoundIndex);

			if(!alwaysUseDefaultSlime){
				if (slimeRenderer != null) {
					slimeRenderer.material = slimeData.slimeMaterial;
				}

				SlimeSettings.Instance.SetSettings(slimeData.slimeSettingsIndex);
			}else{
				SlimeSettings.Instance.SetSettings(30);
			}

			if (slimeData.itemsIndex != -1) {
				soc.SetItem(slimeData.itemsIndex, slimeData.isItems2D);
				soc.CreateObjects(slimeData.itemsCount);
			}
		}

		public void CloseSlime() {
			ApplyDefaultSettings();
			slimeContainer.SetActive(false);
			slimeUI.SetActive(false);
			isStarted = false;
		}

		[Header("Helper_SlimeMaterials")]
		public Material[] slimeMaterials;
		public Texture2D[] tSlimeAlbedos;
		public Texture2D[] tSlimeNormals;

		[ContextMenu("FillMaterialsForSlime")]
		public void FillMaterialsForSlime() {
			for (int i = 0; i < slimeMaterials.Length; i++) {
				if (i < tSlimeAlbedos.Length) {
					slimeMaterials[i].SetTexture("_MainTex", tSlimeAlbedos[i]);
				}

				if (i < tSlimeNormals.Length) {
					slimeMaterials[i].SetTexture("_BumpMap", tSlimeNormals[i]);
				}
			}
		}

		[ContextMenu("RenameTexturesOnMaterials")]
		public void RenameTexturesOnMaterials() {
			//for (int i = 0; i < slimeMaterials.Length; i++) {
			//	Debug.Log("MaterialName: " + slimeMaterials[i].name);

			//	var path = AssetDatabase.GetAssetPath(slimeMaterials[i].GetTexture("_MainTex"));
			//	Debug.Log("_MainTex: " + path);
			//	AssetDatabase.RenameAsset(path, "SlimeTexture_" + (i + 1) + "_a");

			//	path = AssetDatabase.GetAssetPath(slimeMaterials[i].GetTexture("_BumpMap"));//.GUIDToAssetPath(tSlimeNormals[i].GetInstanceID().ToString());
			//	Debug.Log("_BumpMap: " + path);
			//	AssetDatabase.RenameAsset(path, "SlimeTexture_" + (i + 1) + "_n");
			//}

			//AssetDatabase.Refresh();
		}

		//[Header("Helper_SlimeTradeInfo")]
		//public PopIt.PopTradeInfo[] slimeTradeInfo;
		//public Sprite[] tradeIcons;
		//public GameObject[] tradeTablePrefabs;
		//public int[] tradeSortingIndexes;

		//[ContextMenu("FillSlimeTradeInfos")]
		//public void FillSlimeTradeInfos() {
		//	for (int i = 0; i < slimeTradeInfo.Length; i++) {
		//		if (i < tradeIcons.Length && tradeIcons[i] != null) {
		//			slimeTradeInfo[i].shopIcon = tradeIcons[i];
		//		}

		//		if (i < tradeTablePrefabs.Length && tradeTablePrefabs[i] != null) {
		//			slimeTradeInfo[i].shopModel = tradeTablePrefabs[i];
		//		}

		//		slimeTradeInfo[i].prefsName = "ShopTradeItem_" + (i + 1);
		//		slimeTradeInfo[i].analythicName = "ShopTradeItem_" + (i + 1);
		//		slimeTradeInfo[i].minigameIndex = i;

		//		if (i < tradeSortingIndexes.Length && tradeSortingIndexes[i] != 0) {
		//			slimeTradeInfo[i].sortingIndex = tradeSortingIndexes[i];
		//		}

		//		slimeTradeInfo[i].SetDirty();
		//	}
		//}

		public SlimeSettings ssPrefab;

		[ContextMenu("GenerateSlimeSettingsIndexes")]
		public void GenerateSlimeSettingsIndexes() {
			for (int i = 0; i < slimesData.list.Count; i++) {
				slimesData.list[i].slimeSettingsIndex = Random.Range(0, ssPrefab.slimeSettings.Length);
			}

			slimesData.SetDirty();
		}
	}
}