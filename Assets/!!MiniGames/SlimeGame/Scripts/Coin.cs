using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pet.Slime{
	public class Coin : MonoBehaviour{

		int indexVertex = -1;
		SlimeMeshDeformer SMD;

		public bool isCrystal;

		void Start() {
			if (SlimeGame.Instance.SMD) {
				indexVertex = GetComponent<SwimmingObject>().numberOfVertex;
				SMD = SlimeGame.Instance.SMD;
			} else {
				Destroy(this);
			}
		}

		void Update() {
			if (SMD.isVertexUnderFinger[indexVertex] == true) {
				Take();
			}
		}

		public int moneyRewardValue = 10;
		public int curRewardMoney = 10;

		public int crystalRewardValue = 1;
		public int curRewardCrystal = 1;

		void Take() {
			//if (InAppRewardController.VIPStatusActive) {
			//	curRewardMoney = moneyRewardValue * 2;
			//	curRewardCrystal = crystalRewardValue * 2;
			//}

			if (isCrystal) {
				//if (CustomCurrencyData.Instance) {
				//	CustomCurrencyData.Instance.AddCustomCurrency(curRewardCrystal);
				//}
				//Destroy(gameObject);
			} else {
				//SwimmingObjectsController.Instance.coinSound.Play();
				//if (!SlimeGame.Instance.isBonusActivate) {
				//	CurrencyManager.Instance.AddCoins(curRewardMoney);
				//} else {
				//	CurrencyManager.Instance.AddCoins(curRewardMoney * 2);
				//}
				Destroy(gameObject);
			}
		}
	}
}