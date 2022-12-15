using UnityEngine;

namespace Pet.Slime{
    public class SlimeShowHideUI : MonoBehaviour{
        [Header("Tween Positions")]
        public TweenPosition[] tweenPositions;

        public SlimeShowHideUI_Tween[] tweens;

        [Header("Speed")]
        public float speed = 1f;

        SlimeMeshDeformer SMD;
        bool isTouch;

        void Start() {
            SMD = SlimeMeshDeformer.Instance;

            tweens = new SlimeShowHideUI_Tween[tweenPositions.Length];
            for (int i = 0; i < tweenPositions.Length; i++) {
                tweens[i] = new SlimeShowHideUI_Tween();
                tweens[i].Init(this, tweenPositions[i]);
            }
        }

        void PlayTween(bool isHideUI) {
            for (int i = 0; i < tweens.Length; i++) {
                tweens[i].PlayTween(isHideUI);
            }
        }


        void LateUpdate() {
            if (SMD == null) {
                //currentAlpha = 1f;
                //ApplyAlpha();
                enabled = false;
                return;
            }

            if (!SlimeGame.Instance.isStarted) {
                return;
            }

            if (SMD.IsMeshTouched() && !isTouch) {
                PlayTween(true);
                isTouch = true;
            } else if (!SMD.IsMeshTouched() && isTouch) {

                PlayTween(false);

                isTouch = false;
            }
        }

        [System.Serializable]
        public class SlimeShowHideUI_Tween
        {
            SlimeShowHideUI controller;

            public TweenPosition tweenPos;
            public Vector3 from;
            public Vector3 to;
            public float magnitude;

            public void Init(SlimeShowHideUI _controller, TweenPosition _tweenPos) {
                controller = _controller;

                tweenPos = _tweenPos;
                from = tweenPos.from;
                to = tweenPos.to;

                magnitude = (from - to).magnitude;
            }

            public void PlayTween(bool isHideUI) {
                tweenPos.from = tweenPos.transform.localPosition;
                tweenPos.to = isHideUI ? to : from;
                tweenPos.duration = (tweenPos.from - tweenPos.to).magnitude / magnitude / controller.speed + Random.Range(0.001f, 0.002f);


                tweenPos.ResetToBeginning();
                if (tweenPos.enabled) {
                    tweenPos.tweenFactor = 0.4f;
                }
                tweenPos.PlayForward();
            }
        }
    }
}