using System.Collections.Generic;
using UnityEngine;

namespace Pet.Slime{
	[CreateAssetMenu(menuName = "Slime/SlimeData_List")]
	public class SlimeData_List : ScriptableObject{
		public List<SlimeData> list = new List<SlimeData>();
	}

	[System.Serializable]
	public class SlimeData{
		public Material slimeMaterial;
		public int slimeSoundIndex = 0;
		public int slimeSettingsIndex = 0;
		public int itemsIndex = -1;
		public int itemsCount = 0;
		public bool isItems2D = true;
	}
}