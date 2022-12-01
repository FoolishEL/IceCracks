using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ProtoTurtle.BitmapDrawing;

namespace IceCracks.CracksGeneration.Generation
{
    public class CrackVisualizer : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;

        [SerializeField] private Color color;
        [SerializeField]
        private Texture2D texture2D;
        private RectTransform rectTransform;

        private bool _isInited = false;

        public void Initialize()
        {
            if (_isInited)
                return;
            _isInited = true;
            rectTransform = transform as RectTransform;
            if (texture2D == null)
                texture2D = new Texture2D((int)rectTransform.sizeDelta.x, (int)rectTransform.sizeDelta.y);
            else
            {
                Texture2D copyTexture = new Texture2D(texture2D.width, texture2D.height);
                copyTexture.SetPixels(texture2D.GetPixels());
                copyTexture.Apply();
                texture2D = copyTexture;
            }
            rawImage.texture = texture2D;
        }

        public async Task DrawCracks(IEnumerable<(Vector2Int, Vector2Int)> lines, Vector2Int center)
        {
            //TODO: refactor this
            lines = lines.OrderBy(c => Vector2Int.Distance(center, c.Item1));
            var first = lines.Where((item, index) => index % 3 == 0);
            var second = lines.Where((item, index) => index % 3 == 1);
            var third = lines.Where((item, index) => index % 3 == 2);
            await Task.WhenAll(RawDraw(first), RawDraw(second), RawDraw(third));
        }

        private async Task RawDraw(IEnumerable<(Vector2Int, Vector2Int)> lines)
        {
            foreach (var line in lines)
            {
                texture2D.DrawLine(line.Item1, line.Item2, color);
                texture2D.Apply();
                await Task.Yield();
            }
        }

        public void DebugDrawPoint(Vector2Int position)
        {
            texture2D.DrawFilledCircle(position.x, position.y, 5, color);
            texture2D.Apply();
        }

        public Vector2Int GetTextureSize()
        {
            if (rectTransform is not null)
            {
                var sizeDelta = rectTransform.sizeDelta;
                return new Vector2Int((int)sizeDelta.x, (int)sizeDelta.y);
            }

            return Vector2Int.zero;
        }

        public void Restart()
        {
            texture2D = new Texture2D((int)rectTransform.sizeDelta.x, (int)rectTransform.sizeDelta.y);
            rawImage.texture = texture2D;
        }
    }
}