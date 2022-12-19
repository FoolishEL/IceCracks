using System;
using System.Collections.Generic;
using UnityEngine;

namespace IceCracks.CracksGeneration
{
    using Extensions;
    using Models;
    using Views;
    
    public class MeshCrackGenerator : MonoBehaviour
    {
        [SerializeField] private MeshCollider meshCollider;
        private MeshCrackVisualizer crackVisualizer;

        public Bounds bounds { get; private set; }
        private bool isInitialized;
        private Camera raycastCamera;

        private CrackModel model;
        public CrackModel Model => model;
        private float delay = .01f;
        private float lastTimeClicked;


        public void Initialize(MeshCrackVisualizer meshCrackVisualizer, Camera raycastCamera)
        {
            lastTimeClicked = Time.time - delay;
            if (isInitialized)
                return;
            this.raycastCamera = raycastCamera;
            crackVisualizer = meshCrackVisualizer;
            bounds = meshCollider.bounds;
            isInitialized = true;
            CreateData();
        }

        private void CreateData()
        {
            model = new CrackModel(crackVisualizer.GetTextureSize());
        }

        private void OnMouseDown() => GetCrackAction(1f);

        private void GetCrackAction(float percentage)
        {
            if (Time.time - lastTimeClicked < delay)
                return;
            lastTimeClicked = Time.time;
            Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100))
            {
                if (bounds.Contains(hit.point))
                {
                    var relativePosition = GetRelativePositionOnMesh(hit.point);
                    //bool isProlonged =
                    model.AddCracks(relativePosition, CrackExtensions.TOKEN_DEBUG_INITIAL_CRACK_FORCE, percentage);
                    //crackVisualizer.DrawCracks(model.GetPoints(), GetPressPosition(relativePosition), isProlonged);
                    CrackSoundPLayer.Instance.PlayCrack();
                }
            }
        }

        private void OnMouseEnter()
        {
            GetCrackAction(1f);
        }

        private void OnMouseDrag()
        {
            GetCrackAction(1f);
        }

        private Vector2Int GetPressPosition(Vector2 relativePos)
        {
            var textureSize = crackVisualizer.GetTextureSize();
            relativePos.x *= textureSize.x;
            relativePos.y *= textureSize.y;
            return new Vector2Int((int)relativePos.x, textureSize.y - (int)relativePos.y);
        }

        private Vector2 GetRelativePositionOnMesh(Vector3 initialPosition)
        {
            return new Vector2((initialPosition.x - bounds.min.x) / (bounds.max.x - bounds.min.x),
                (initialPosition.z - bounds.min.z) / (bounds.max.z - bounds.min.z));
        }
    }
}