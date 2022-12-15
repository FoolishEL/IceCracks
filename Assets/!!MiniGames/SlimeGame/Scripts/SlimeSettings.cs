using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pet.Slime
{
    public class SlimeSettings : MonoBehaviour
    {

        static SlimeSettings __instance;
        public static SlimeSettings Instance {
            get {
                if (!__instance) {
                    __instance = Instantiate(Resources.Load<SlimeSettings>("SlimeSettings"));//.GetComponent<SlimeSettings>();
                }
                return __instance;
            }
        }

        [Header("Slime Settings")]
        public SlimeSetting[] slimeSettings;

        [Header("Current Settings")]
        public SlimeSetting currentSettings;

        [Header("Menu")]
        public bool isSceneMenu;
        public int startSettingsIndex;

        [Header("Generation")]
        public List<int> ignoreGenerate = new List<int>();

        private void Awake() {
            __instance = this;
        }

        void Start() {
            if (isSceneMenu) {
                SetSettings(startSettingsIndex);
            }
        }


        [ContextMenu("Generate Settings")]
        public void GenerateSettings() {
            for (int i = 0; i < slimeSettings.Length; i++) {
                if (ignoreGenerate.Contains(i)) {
                    continue;
                }

                if (Random.Range(0, 2) == 0) {
                    slimeSettings[i] = new SlimeSetting(GetNewRandomizeSlime(), slimeSettings[i].name);
                } else {
                    slimeSettings[i] = new SlimeSetting(GetSimilarSlime(ignoreGenerate[Random.Range(0, ignoreGenerate.Count)]), slimeSettings[i].name);
                }

            }
        }

        SlimeSetting GetSimilarSlime(int _from) {
            SlimeSetting slime = new SlimeSetting(slimeSettings[_from]);


            float percent = Random.Range(0.1f, 0.3f);// Плюс минус 10-30 %

            slime.velocityDecay += Random.Range(-1f, 1f) * percent * slime.velocityDecay;
            slime.stretchFactor += Random.Range(-1f, 1f) * percent * slime.stretchFactor;
            slime.physicsSpeed += Random.Range(-1f, 1f) * percent * slime.physicsSpeed;
            slime.tightnessFactor += Random.Range(-1f, 1f) * percent * slime.tightnessFactor;

            slime.materialStickiness += Random.Range(-1f, 1f) * percent * slime.materialStickiness;
            slime.pokeEffect += Random.Range(-1f, 1f) * percent * slime.pokeEffect;

            slime.relativeMovementConstraint += Random.Range(-1f, 1f) * percent * slime.relativeMovementConstraint;

            return slime;
        }

        SlimeSetting GetNewRandomizeSlime() {
            SlimeSetting slime = new SlimeSetting();
            slime.velocityDecay = Random.Range(0f, 0.05f);
            slime.stretchFactor = Random.Range(0.003f, 0.03f);
            slime.physicsSpeed = Random.Range(0.6f, 1f);
            slime.tightnessFactor = Random.Range(0.8f, 1.1f);

            slime.isFixedDuringSwipe = (Random.Range(0, 2) == 0) ? true : false;

            // stickenss и poke
            if (Random.Range(0, 3) == 0) {
                if (Random.Range(0, 2) == 0) {
                    slime.materialStickiness = Random.Range(0f, 0.2f);
                    slime.pokeEffect = 0f;
                } else {
                    slime.materialStickiness = 0f;
                    slime.pokeEffect = Random.Range(0f, 0.2f);
                }
            } else {
                slime.materialStickiness = 0f;
                slime.pokeEffect = 0f;
            }

            slime.relativeMovementConstraint = Random.Range(0f, 1f);

            return slime;
        }


        public void SetSettings(int num) {
            //Debug.Log("SettingsIndex :" + num);

            //currentSettings = slimeSettings [num];
            currentSettings = new SlimeSetting(slimeSettings[num], slimeSettings[num].name);

            if ((num == 116) || (num == 117)) {
                Application.targetFrameRate = 30;
            }

            SlimeMeshDeformer SMD = SlimeMeshDeformer.Instance;
            if (SMD) {
                SMD.refractionLevel = currentSettings.refractionLevel;

                SMD.velocityDecay = currentSettings.velocityDecay;
                SMD.stretchFactor = currentSettings.stretchFactor;
                SMD.physicsSpeed = currentSettings.physicsSpeed;
                SMD.tightnessFactor = currentSettings.tightnessFactor;

                SMD.isFixedDuringSwipe = currentSettings.isFixedDuringSwipe;

                SMD.materialStickiness = currentSettings.materialStickiness;
                SMD.pokeEffect = currentSettings.pokeEffect;

                SMD.relativeMovementConstraint = currentSettings.relativeMovementConstraint;
                SMD.fingerPressStrength = currentSettings.fingerPressStrength;
                SMD.fingerStrength = currentSettings.fingerStrength;
                SMD.fingerRadius = currentSettings.fingerRadius;
                SMD.isFlipProtection = currentSettings.isFlipProtection;

                if (currentSettings.isInitForces) {
                    SMD.ApplyInitialForces();
                }
            }
        }

    }

    [System.Serializable]
    public class SlimeSetting
    {
        public string name;

        public bool isInitForces = true;

        [Range(-5f, 5f)]
        public float refractionLevel;

        [Header("Physics")]
        //[Range(0f, 1f)]
        public float velocityDecay;

        //[Range(0.001f, 0.03f)]
        public float stretchFactor;

        //[Range(0.6f, 1f)]
        public float physicsSpeed;

        //[Range(0.8f, 2f)]
        public float tightnessFactor;

        public bool isFixedDuringSwipe;

        //[Range(0f, 0.25f)]
        public float materialStickiness;

        //[Range(0f, 0.25f)]
        public float pokeEffect;



        [Header("Finger")]
        //[Range(0f, 1f)]
        public float relativeMovementConstraint;
        //[Range(-3f, 3f)]
        public float fingerPressStrength = 1f;
        //[Range(0f, 2f)]
        public float fingerStrength = 1f;
        //[Range(0.2f, 4f)]
        public float fingerRadius = 1f;
        public bool isFlipProtection;


        public SlimeSetting() {

        }

        public SlimeSetting(SlimeSetting s, string _name = "") {
            name = _name;

            isInitForces = s.isInitForces;
            refractionLevel = s.refractionLevel;

            velocityDecay = s.velocityDecay;
            stretchFactor = s.stretchFactor;
            physicsSpeed = s.physicsSpeed;
            tightnessFactor = s.tightnessFactor;
            isFixedDuringSwipe = s.isFixedDuringSwipe;
            materialStickiness = s.materialStickiness;
            pokeEffect = s.pokeEffect;
            relativeMovementConstraint = s.relativeMovementConstraint;
            fingerPressStrength = s.fingerPressStrength;
            fingerStrength = s.fingerStrength;
            fingerRadius = s.fingerRadius;
            isFlipProtection = s.isFlipProtection;
        }

    }
}