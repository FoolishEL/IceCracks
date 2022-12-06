//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IceCracks.CracksGeneration.Generation
{
    using Extensions;
    using Models;
    [RequireComponent(typeof(Graphic))]
    public class CrackGenerator : MonoBehaviour, IPointerClickHandler
    {
        /*
        [SerializeField] private float minStep = .1f;
        [SerializeField] private float maxStep = 1f;
        */

        private CrackVisualizer crackVisualizer;
        private RectTransform rectTransform;
        //private List<(Vector2, Vector2)> lines;
        private bool isInitialized;

        private CrackModel model;

        public void Initialize(CrackVisualizer visualizer)
        {
            if (isInitialized)
                return;
            isInitialized = true;
            
            crackVisualizer = visualizer;
            CreateData();
            rectTransform = transform as RectTransform;
        }

        public void Restart()
        {
            CreateData();
        }

        private void CreateData()
        {
            //lines = new List<(Vector2, Vector2)>();
            model = new CrackModel(crackVisualizer.GetTextureSize());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 relativeToCenter = eventData.position - (Vector2)transform.position;
            var sizeDelta = rectTransform.sizeDelta;
            Vector2 relativeToBottomLeft = relativeToCenter + sizeDelta / 2;
            Vector2Int pos = new Vector2Int((int)relativeToBottomLeft.x, (int)relativeToBottomLeft.y);
            pos.y = (int)sizeDelta.y - pos.y;
            model.AddCracks(pos, CrackExtensions.TOKEN_DEBUG_INITIAL_CRACK_FORCE);
            crackVisualizer.DrawCracks(model.GetPoints(), pos);
        }
    }
}