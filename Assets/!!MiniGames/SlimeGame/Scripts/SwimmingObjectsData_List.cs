using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pet.Slime{
    [CreateAssetMenu(menuName = "Slime/SwimmingObjectsData_List")]
    public class SwimmingObjectsData_List : MonoBehaviour{
        public List<SwimmingObjectsData> list = new List<SwimmingObjectsData>();
    }

    [System.Serializable]
    public class SwimmingObjectsData{
        public Sprite soSprite;
        public GameObject so3DPrefab;
    }


}