using Spaccanavi.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Spaccanavi.Audio
{
    public sealed class AudioManager : MonoBehaviour, ISingleton
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioEntry[] audios;

        private readonly Dictionary<string, AudioSource> audioSourceDict = new Dictionary<string, AudioSource>();



        private void Awake()
        {
            Instance = this;

            for (int i = 0; i < audios.Length; i++)
            {
                AudioEntry audio = audios[i];

                AudioSource source = gameObject.AddComponent<AudioSource>();

                source.clip = audio.AudioClip;
                source.outputAudioMixerGroup = audio.OutputMixerGroup;
                source.bypassEffects = audio.BypassEffects;
                source.bypassListenerEffects = audio.BypassListenerEffects;
                source.bypassReverbZones = audio.BypassReverbZones;
                source.playOnAwake = audio.PlayOnAwake;
                source.loop = audio.Loop;
                source.priority = audio.Priority;
                source.volume = audio.Volume;
                source.pitch = audio.Pitch;
                source.panStereo = audio.StereoPan;
                source.spatialBlend = audio.SpatialBlend;
                source.reverbZoneMix = audio.ReverbZoneMix;

                audioSourceDict.Add(audio.Key, source);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }



#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < audios.Length; i++)
            {
                if (!audios[i].IsInitialized)
                {
                    audios[i] = new AudioEntry();
                    audios[i].IsInitialized = true;
                }
            }
        }
#endif



        public void Play(string key)
        {
            if (!audioSourceDict.ContainsKey(key))
                throw new ArgumentException($"The audio manager doesn't have a audio source with key, \"{key}\".");

            audioSourceDict[key].Play();
        }

        public void Stop(string key)
        {
            if (!audioSourceDict.ContainsKey(key))
                throw new ArgumentException($"The audio manager doesn't have a audio source with key, \"{key}\".");

            audioSourceDict[key].Stop();
        }

        public void Pause(string key)
        {
            if (!audioSourceDict.ContainsKey(key))
                throw new ArgumentException($"The audio manager doesn't have a audio source with key, \"{key}\".");

            audioSourceDict[key].Pause();
        }

        public void Unpause(string key)
        {
            if (!audioSourceDict.ContainsKey(key))
                throw new ArgumentException($"The audio manager doesn't have a audio source with key, \"{key}\".");

            audioSourceDict[key].UnPause();
        }

        public bool IsPlaying(string key)
        {
            if (!audioSourceDict.ContainsKey(key))
                throw new ArgumentException($"The audio manager doesn't have a audio source with key, \"{key}\".");

            return audioSourceDict[key].isPlaying;
        }



        [Serializable]
        private sealed record AudioEntry
        {
            [SerializeField] private string key;
            [SerializeField] private AudioClip audioClip;
            [SerializeField] private AudioMixerGroup outputMixerGroup;
            [SerializeField] private bool bypassEffects;
            [SerializeField] private bool bypassListenerEffects;
            [SerializeField] private bool bypassReverbZones;
            [SerializeField] private bool playOnAwake;
            [SerializeField] private bool loop;
            [SerializeField, Range(0, 256)] private int priority = 128;
            [SerializeField, Range(0f, 1f)] private float volume = 1f;
            [SerializeField, Range(-3f, 3f)] private float pitch = 1f;
            [SerializeField, Range(-1f, 1f)] private float stereoPan = 0f;
            [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f;
            [SerializeField, Range(0f, 1.1f)] private float reverbZoneMix = 1f;

            public string Key => key;
            public AudioClip AudioClip => audioClip;
            public AudioMixerGroup OutputMixerGroup => outputMixerGroup;
            public bool BypassEffects => bypassEffects;
            public bool BypassListenerEffects => bypassListenerEffects;
            public bool BypassReverbZones => bypassReverbZones;
            public bool PlayOnAwake => playOnAwake;
            public bool Loop => loop;
            public int Priority => priority;
            public float Volume => volume;
            public float Pitch => pitch;
            public float StereoPan => stereoPan;
            public float SpatialBlend => spatialBlend;
            public float ReverbZoneMix => reverbZoneMix;

#if UNITY_EDITOR
            [SerializeField, HideInInspector] private bool isInitialized;

            public bool IsInitialized { get => isInitialized; set => isInitialized = value; }
#endif
        }
    }
}