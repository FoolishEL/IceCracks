using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean;

public class ParticlesOnSlime : MonoBehaviour {

    //public List<ParticleSystem> particles = new List<ParticleSystem>();
    [Header("Is Enable Particles")]
    public bool isEmit = true;

    public Transform particleTR;
    public ParticleSystem particle;

    public Camera slimeCamera;

    //[Header("Instantiate")]
    //public GameObject prefab;
    //public GameObject prefabPath;

    [Header("Emit Delta Time")]
    public float emitDeltaTime = 0.1f;
    public float emitTimer;

    private List<LeanFinger> fingers = new List<LeanFinger>();

    void Start() {
        emitTimer = emitDeltaTime;
    }

    void Update() {
        if (!isEmit) {
            return;
        }

        emitTimer -= Time.unscaledDeltaTime;
        if(emitTimer < 0) {

            Emit();
            emitTimer = emitDeltaTime;
        }

    }



    LeanFinger _finger;
    Vector3 _worldPosition;
    int _countParticles;
    void Emit() {
        fingers = LeanTouch.Fingers;

        for (int i = 0; i < fingers.Count; i++) {
            _finger = fingers[i];

            _worldPosition = _finger.GetLastWorldPosition(8.5f, slimeCamera);

            _countParticles = Mathf.Clamp(Mathf.RoundToInt(_finger.ScaledDeltaScreenPosition.magnitude / 5f) , 1 , 10);

            particleTR.transform.position = _worldPosition;
            particle.Emit(_countParticles);

            //Debug.Log(finger.ScaledDeltaScreenPosition.magnitude);
        }

    }

}
