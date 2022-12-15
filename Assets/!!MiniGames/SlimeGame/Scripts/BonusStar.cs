using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pet.Slime{
	public class BonusStar : MonoBehaviour{
		public GameObject star3d;

		public TypeBonusInGame type;

		public float timeOff;
		public float timer_off;

		void Start() {
			timer_off = timeOff;
		}

		//void Update() {
		//	if (timer_off > 0) {
		//		if (star3d.activeSelf) {
		//			timer_off -= Time.deltaTime;
		//			if (timer_off <= 0) {
		//				BonusSetActive(false);
		//			}
		//		}
		//	}
		//}

		public void Check() {
			//if(!BroGame.Instance.IsCanGenerateCoinAndCrystal()){
			//	BonusSetActive (false);
			//	return;
			//}

			if (type == TypeBonusInGame.X2_Coin) {
				if (SlimeGame.Instance.isBonusActivate) {
					BonusSetActive(false);
				}
			}

			//if (type == TypeBonusInGame.X2_Crystal) {
			//	if (SlimeGame.Instance.X2_Spawn_Crystal) {
			//		BonusSetActive(false);
			//	}
			//}
		}

		public void BonusSetActive(bool _isActive) {
			if (_isActive) {
				if (SlimeGame.Instance != null) {
					//				if ((BroGame.Instance.mode == 3) || (BroGame.Instance.mode == 5)) {
					//					return;
					//				}
					//if (!SlimeGame.Instance.IsCanGenerateCoinAndCrystal()) {
					//	return;
					//}

					if (type == TypeBonusInGame.X2_Coin) {
						if (SlimeGame.Instance.isBonusActivate) {
							return;
						}
					}

					//if (type == TypeBonusInGame.X2_Crystal) {
					//	if (BroGame.Instance.X2_Spawn_Crystal) {
					//		return;
					//	}
					//}
				}
				timer_off = timeOff;
			}

			star3d.SetActive(_isActive);
			GetComponent<BoxCollider>().enabled = _isActive;
		}

		void OnClick() {

			//		if (PlayerPrefs.GetInt ("Free_Bonus_Was_Received", 0) == 0) {
			//			PlayerPrefs.SetInt ("Free_Bonus_Was_Received", 1);
			//			GetComponent<TimeBasedItem> ().OnVideoShown ();
			//		} else {
			//GetComponent<TimeBasedItem>().ShowUserDialog();
			//		}

			//BonusSetActive(false);
			//if (GetComponent<TimeBasedItem>().available) {
			//	GetComponent<TimeBasedItem>().UpdateLastTime();
			//	GetComponent<TimeBasedItem>().available = false;
			//	GetComponent<TimeBasedItem>().UpdateEffect();
			//}
			//		BroGame.Instance.CloseAllBonuses();

			//RewardedADSManager.Instance.AdsX2CoinsSlime();

			// Open x2 dialog
		}

		public void ActivateBonusButton() {
			//RewardedADSManager.Instance.AdsX2CoinsSlime();
		}

		[ContextMenu("ActivateBonus")]
		public void ActivateBonus() {
			//		BroGame.Instance.ActivateBonus ();

			switch (type) {
				case TypeBonusInGame.X2_Coin:
					SlimeGame.Instance.ActivateBonus();
					//			BroGame.Instance.Activate_X2_Coin ();
					break;
				//case TypeBonusInGame.X2_Crystal:
				//	BroGame.Instance.Activate_X2_Crystal();
				//	break;
			}
		}
	}

	public enum TypeBonusInGame
	{
		X2_Coin,
		X2_Crystal
	}
}