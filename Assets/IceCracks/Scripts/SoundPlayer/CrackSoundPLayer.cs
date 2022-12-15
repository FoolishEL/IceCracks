using System;
using System.Collections;
using System.Collections.Generic;
using IceCracks.Settings;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class CrackSoundPLayer : MonoBehaviour
{
    [SerializeField] private List<AudioClip> cracksAudioClip;

    [SerializeField] private List<AudioClip> iceFreeze;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private CrackGameplaySettings settings;

    public static CrackSoundPLayer Instance { get; private set; }

    private ObjectPool<AudioSource> objectPool;

    private void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            Initialize();
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDestroy() => Instance = null;

    private void Initialize()
    {
        if (settings.TryGetCurrentSettings(out var settingsConfig))
        {
            var value = settingsConfig.Value;
            cracksAudioClip.Clear();
            cracksAudioClip.AddRange(value.audioSetCracks);
            iceFreeze.Clear();
            iceFreeze.AddRange(value.audioSetFreeze);
        }
        objectPool = new ObjectPool<AudioSource>(
            () => Instantiate(audioSource, transform, true),
            (c) => c.gameObject.SetActive(true),
            (c) => c.gameObject.SetActive(false),
            null, true, 6, 60);
    }
    
    [ContextMenu(nameof(PlayCrack))]
    public void PlayCrack()
    {
        StartCoroutine(PLayCrackCoroutine(cracksAudioClip[Random.Range(0, cracksAudioClip.Count)],
            Random.Range(.8f, 1.2f)));
    }
    [ContextMenu(nameof(PlayFreeze))]
    public void PlayFreeze()
    {
        StartCoroutine(PLayCrackCoroutine(iceFreeze[Random.Range(0, cracksAudioClip.Count)],
            Random.Range(.8f, 1.2f)));
    }

    private IEnumerator PLayCrackCoroutine(AudioClip clip,float pitch)
    {
        var source = objectPool.Get();
        source.clip = clip;
        source.pitch = pitch;
        source.Play();
        yield return new WaitForSecondsRealtime(clip.length * 1.2f);
        objectPool.Release(source);
    }
}
