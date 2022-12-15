using System.Collections.Generic;
using ProtoTurtle.BitmapDrawing;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using IceCracks.Math;
using Random = UnityEngine.Random;

namespace IceCracks.Views
{
    using CracksGeneration;
    using Utilities;
    using Settings;
    public class MeshCrackVisualizer : MonoBehaviour
    {
        [SerializeField] private Material materialInitial;
        public Material currentMaterial { get; private set; }

        [SerializeField] private Color color;
        [SerializeField] private Texture2D texture2D;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private CrackGameplaySettings settings;
        [SerializeField] private float speedOfAppear = .7f;

        private MeshCrackGenerator meshCrackGenerator;

        private bool _isInited = false;
        public event Action OnCracksDrawn = delegate { };
        public static event Action OnAppear = delegate { }; 

        [HideInInspector] public bool IsDrawn;

        public void Initialize(MeshCrackGenerator meshCrackGenerator)
        {
            if (_isInited)
                return;
            if (settings.TryGetCurrentSettings(out var settingsConfig))
            {
                var value = settingsConfig.Value;
                texture2D = BMeshUtilities.CopyTexture(value.texture);
                color = value.cracksColor;
            }
            else
            {
                texture2D = BMeshUtilities.CopyTexture(texture2D);
            }
            IsDrawn = true;
            this.meshCrackGenerator = meshCrackGenerator;
            currentMaterial = Instantiate(materialInitial);
            currentMaterial.color =Color.clear;
            currentMaterial.mainTexture = texture2D;
            meshRenderer.materials = new[] { currentMaterial };
            StartCoroutine(SlowAppear());
        }

        private IEnumerator SlowAppear()
        {
            CrackSoundPLayer.Instance.PlayFreeze();
            meshCrackGenerator.SetBusyStatus(this);
            for (float f = 0; f < speedOfAppear; f += Time.deltaTime)
            {
                currentMaterial.color = Color.Lerp(Color.clear, Color.white,f/speedOfAppear);
                // if(MathExtensions.GetRandomWithPercent(.2f))
                //     CrackSoundPLayer.Instance.PlayFreeze();
                yield return null;
            }
            meshCrackGenerator.UnsetBusyStatus(this);
            OnAppear.Invoke();
        }

        public Texture2D GetCurrentTextureCopy() => BMeshUtilities.CopyTexture(texture2D);

        public async Task DrawCracks(IEnumerable<(Vector2Int, Vector2Int)> lines, Vector2Int center, bool isSilent)
        {
            IsDrawn = false;
            meshCrackGenerator.SetBusyStatus(this);
            //TODO: refactor this
            // lines = lines.OrderBy(c => Vector2Int.Distance(center, c.Item1));
            // var first = lines.Where((item, index) => index % 3 == 0);
            // var second = lines.Where((item, index) => index % 3 == 1);
            // var third = lines.Where((item, index) => index % 3 == 2);
            await PlayCrack();
            //await Task.WhenAll(RawDraw(first), RawDraw(second), RawDraw(third));
            meshCrackGenerator.UnsetBusyStatus(this);
            IsDrawn = true;
            if (!isSilent)
                OnCracksDrawn.Invoke();
        }

        private async Task PlayCrack()
        {
            CrackSoundPLayer.Instance.PlayCrack();
            // int randCount = Random.Range(3, 6);
            // for (int i = 0; i < randCount; i++)
            // {
            //     CrackSoundPLayer.Instance.PlayCrack();
            //     await Task.Delay(Random.Range(30, 100));
            // }
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
            return new Vector2Int(texture2D.width, texture2D.height);
        }
    }
}