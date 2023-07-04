using System.Collections;
using System.Collections.Generic;
using CGK.Sound.Model;
using UnityEngine;

namespace CGK.Sound
{
    /// <summary>
    /// Sound manager extension methods
    /// </summary>
    public static class SoundManagerExtensions
    {
        /// <summary>
        /// Play an audio clip once using the global sound volume as a multiplier
        /// </summary>
        /// <param name="source">AudioSource</param>
        /// <param name="clip">Clip</param>
        public static void PlayOneShotSoundManaged(this AudioSource source, AudioClip clip)
        {
            SoundManager.PlayOneShotSound(source, clip, 1.0f);
        }

        /// <summary>
        /// Play an audio clip once using the global sound volume as a multiplier
        /// </summary>
        /// <param name="source">AudioSource</param>
        /// <param name="clip">Clip</param>
        /// <param name="volumeScale">Additional volume scale</param>
        public static void PlayOneShotSoundManaged(this AudioSource source, AudioClip clip, float volumeScale)
        {
            SoundManager.PlayOneShotSound(source, clip, volumeScale);
        }

        /// <summary>
        /// Play an audio clip once using the global music volume as a multiplier
        /// </summary>
        /// <param name="source">AudioSource</param>
        /// <param name="clip">Clip</param>
        public static void PlayOneShotMusicManaged(this AudioSource source, AudioClip clip)
        {
            SoundManager.PlayOneShotMusic(source, clip, 1.0f);
        }

        /// <summary>
        /// Play an audio clip once using the global music volume as a multiplier
        /// </summary>
        /// <param name="source">AudioSource</param>
        /// <param name="clip">Clip</param>
        /// <param name="volumeScale">Additional volume scale</param>
        public static void PlayOneShotMusicManaged(this AudioSource source, AudioClip clip, float volumeScale)
        {
            SoundManager.PlayOneShotMusic(source, clip, volumeScale);
        }

        /// <summary>
        /// Play a sound and loop it until stopped, using the global sound volume as a modifier
        /// </summary>
        /// <param name="source">Audio source to play</param>
        public static void PlayLoopingSoundManaged(this AudioSource source)
        {
            SoundManager.PlayLoopingSound(source, 1.0f, 1.0f);
        }

        /// <summary>
        /// Play a sound and loop it until stopped, using the global sound volume as a modifier
        /// </summary>
        /// <param name="source">Audio source to play</param>
        /// <param name="volumeScale">Additional volume scale</param>
        /// <param name="fadeSeconds">The number of seconds to fade in and out</param>
        public static void PlayLoopingSoundManaged(this AudioSource source, float volumeScale, float fadeSeconds)
        {
            SoundManager.PlayLoopingSound(source, volumeScale, fadeSeconds);
        }

        /// <summary>
        /// Play a music track and loop it until stopped, using the global music volume as a modifier
        /// </summary>
        /// <param name="source">Audio source to play</param>
        public static void PlayLoopingMusicManaged(this AudioSource source)
        {
            SoundManager.PlayLoopingMusic(source, 1.0f, 1.0f, false);
        }

        /// <summary>
        /// Play a music track and loop it until stopped, using the global music volume as a modifier
        /// </summary>
        /// <param name="source">Audio source to play</param>
        /// <param name="volumeScale">Additional volume scale</param>
        /// <param name="fadeSeconds">The number of seconds to fade in and out</param>
        /// <param name="persist">Whether to persist the looping music between scene changes</param>
        public static void PlayLoopingMusicManaged(this AudioSource source, float volumeScale, float fadeSeconds,
            bool persist)
        {
            SoundManager.PlayLoopingMusic(source, volumeScale, fadeSeconds, persist);
        }

        /// <summary>
        /// Stop a looping sound
        /// </summary>
        /// <param name="source">AudioSource to stop</param>
        public static void StopLoopingSoundManaged(this AudioSource source)
        {
            SoundManager.StopLoopingSound(source);
        }

        /// <summary>
        /// Stop a looping music track
        /// </summary>
        /// <param name="source">AudioSource to stop</param>
        public static void StopLoopingMusicManaged(this AudioSource source)
        {
            SoundManager.StopLoopingMusic(source);
        }
    }

    /// <summary>
    /// Do not add this script in the inspector. Just call the static methods from your own scripts or use the AudioSource extension methods.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        private static readonly List<LoopingAudioSource> Music = new List<LoopingAudioSource>();
        private static readonly List<AudioSource> MusicOneShot = new List<AudioSource>();
        private static readonly List<LoopingAudioSource> Sounds = new List<LoopingAudioSource>();
        private static readonly HashSet<LoopingAudioSource> PersistedSounds = new HashSet<LoopingAudioSource>();

        private static readonly Dictionary<AudioClip, List<float>> SoundsOneShot =
            new Dictionary<AudioClip, List<float>>();

        private static int _persistTag;
        private static bool _needsInitialize = true;
        private static GameObject _root;
        private static SoundManager _instance;
        private static float _soundVolume = 1.0f;
        private static float _musicVolume = 1.0f;
        private static bool _updated;
        private static bool _pauseSoundsOnApplicationPause = true;

        /// <summary>
        /// Maximum number of the same audio clip to play at once
        /// </summary>
        public static int MaxDuplicateAudioClips = 4;

        /// <summary>
        /// Whether to stop sounds when a new level loads. Set to false for additive level loading.
        /// </summary>
        public static bool StopSoundsOnLevelLoad = true;

        private static void EnsureCreated()
        {
            if (!_needsInitialize)
            {
                return;
            }
            _needsInitialize = false;
            _root = new GameObject
            {
                name = "DigitalRubySoundManager",
                hideFlags = HideFlags.HideAndDontSave
            };
            _instance = _root.AddComponent<SoundManager>();
            DontDestroyOnLoad(_root);
        }

        private void StopLoopingListOnLevelLoad(IList<LoopingAudioSource> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!list[i].Persist || !list[i].AudioSource.isPlaying)
                {
                    list.RemoveAt(i);
                }
            }
        }

        private void ClearPersistedSounds()
        {
            foreach (LoopingAudioSource s in PersistedSounds)
            {
                if (!s.AudioSource.isPlaying)
                {
                    Destroy(s.AudioSource.gameObject);
                }
            }

            PersistedSounds.Clear();
        }

        private void SceneManagerSceneLoaded(UnityEngine.SceneManagement.Scene s,
            UnityEngine.SceneManagement.LoadSceneMode m)
        {
            // Just in case this is called a bunch of times, we put a check here
            if (_updated && StopSoundsOnLevelLoad)
            {
                _persistTag++;

                _updated = false;

                Debug.LogWarningFormat("Reloaded level, new sound manager persist tag: {0}", _persistTag);

                StopNonLoopingSounds();
                StopLoopingListOnLevelLoad(Sounds);
                StopLoopingListOnLevelLoad(Music);
                SoundsOneShot.Clear();
                ClearPersistedSounds();
            }
        }

        private void Start()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManagerSceneLoaded;
        }

        private void Update()
        {
            _updated = true;

            for (int i = Sounds.Count - 1; i >= 0; i--)
            {
                if (Sounds[i].Update())
                {
                    Sounds.RemoveAt(i);
                }
            }

            for (int i = Music.Count - 1; i >= 0; i--)
            {
                bool nullMusic = (Music[i] == null || Music[i].AudioSource == null);
                if (nullMusic || Music[i].Update())
                {
                    if (!nullMusic && Music[i].Tag != _persistTag)
                    {
                        Debug.LogWarning("Destroying persisted audio from previous scene: " +
                                         Music[i].AudioSource.gameObject.name);

                        // cleanup persisted audio from previous scenes
                        GameObject.Destroy(Music[i].AudioSource.gameObject);
                    }

                    Music.RemoveAt(i);
                }
            }

            for (int i = MusicOneShot.Count - 1; i >= 0; i--)
            {
                if (!MusicOneShot[i].isPlaying)
                {
                    MusicOneShot.RemoveAt(i);
                }
            }
        }

        private void OnApplicationFocus(bool paused)
        {
            if (SoundManager.PauseSoundsOnApplicationPause)
            {
                if (paused)
                {
                    SoundManager.ResumeAll();
                }
                else
                {
                    SoundManager.PauseAll();
                }
            }
        }

        private static void UpdateSounds()
        {
            foreach (LoopingAudioSource s in Sounds)
            {
                s.TargetVolume = s.OriginalTargetVolume * _soundVolume;
            }
        }

        private static void UpdateMusic()
        {
            foreach (LoopingAudioSource s in Music)
            {
                if (!s.Stopping)
                {
                    s.TargetVolume = s.OriginalTargetVolume * _musicVolume;
                }
            }

            foreach (AudioSource s in MusicOneShot)
            {
                s.volume = _musicVolume;
            }
        }

        private static IEnumerator RemoveVolumeFromClip(AudioClip clip, float volume)
        {
            yield return new WaitForSeconds(clip.length);

            List<float> volumes;
            if (SoundsOneShot.TryGetValue(clip, out volumes))
            {
                volumes.Remove(volume);
            }
        }

        private static void PlayLooping(AudioSource source, List<LoopingAudioSource> sources, float volumeScale,
            float fadeSeconds, bool persist, bool stopAll)
        {
            EnsureCreated();

            CheckDuplicateLoopingAudioSource(source, sources, stopAll);


            GameObject sourceGameObject = source.gameObject;
            sourceGameObject.SetActive(true);
            LoopingAudioSource s = new LoopingAudioSource(source, fadeSeconds, fadeSeconds, persist);
            s.Play(volumeScale, true);
            s.Tag = _persistTag;
            sources.Add(s);

            if (!persist)
            {
                return;
            }

            if (!sourceGameObject.name.StartsWith("PersistedBySoundManager-"))
            {
                sourceGameObject.name = "PersistedBySoundManager-" + sourceGameObject.name + "-" +
                                        sourceGameObject.GetInstanceID();
            }

            sourceGameObject.transform.parent = null;
            GameObject.DontDestroyOnLoad(sourceGameObject);
            PersistedSounds.Add(s);
        }

        private static void CheckDuplicateLoopingAudioSource(AudioSource source, List<LoopingAudioSource> sources,
            bool stopAll)
        {
            for (int i = sources.Count - 1; i >= 0; i--)
            {
                LoopingAudioSource s = sources[i];
                if (s.AudioSource == source)
                {
                    sources.RemoveAt(i);
                }

                if (stopAll)
                {
                    s.Stop();
                }
            }
        }

        private static void StopLooping(AudioSource source, List<LoopingAudioSource> sources)
        {
            foreach (LoopingAudioSource s in sources)
            {
                if (s.AudioSource == source)
                {
                    s.Stop();
                    source = null;
                    break;
                }
            }

            if (source != null)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// Play a sound once - sound volume will be affected by global sound volume
        /// </summary>
        /// <param name="source">Audio source</param>
        /// <param name="clip">Clip</param>
        public static void PlayOneShotSound(AudioSource source, AudioClip clip)
        {
            PlayOneShotSound(source, clip, 1.0f);
        }

        /// <summary>
        /// Play a sound once - sound volume will be affected by global sound volume
        /// </summary>
        /// <param name="source">Audio source</param>
        /// <param name="clip">Clip</param>
        /// <param name="volumeScale">Additional volume scale</param>
        public static void PlayOneShotSound(AudioSource source, AudioClip clip, float volumeScale)
        {
            EnsureCreated();

            List<float> volumes;
            if (!SoundsOneShot.TryGetValue(clip, out volumes))
            {
                volumes = new List<float>();
                SoundsOneShot[clip] = volumes;
            }
            else if (volumes.Count == MaxDuplicateAudioClips)
            {
                return;
            }

            float minVolume = float.MaxValue;
            float maxVolume = float.MinValue;
            foreach (float volume in volumes)
            {
                minVolume = Mathf.Min(minVolume, volume);
                maxVolume = Mathf.Max(maxVolume, volume);
            }

            float requestedVolume = (volumeScale * _soundVolume);
            if (maxVolume > 0.5f)
            {
                requestedVolume = (minVolume + maxVolume) / (volumes.Count + 2);
            }
            // else requestedVolume can stay as is

            volumes.Add(requestedVolume);
            source.PlayOneShot(clip, requestedVolume);
            _instance.StartCoroutine(RemoveVolumeFromClip(clip, requestedVolume));
        }

        /// <summary>
        /// Play a looping sound - sound volume will be affected by global sound volume
        /// </summary>
        /// <param name="source">Audio source to play looping</param>
        public static void PlayLoopingSound(AudioSource source)
        {
            PlayLoopingSound(source, 1.0f, 1.0f);
        }

        /// <summary>
        /// Play a looping sound - sound volume will be affected by global sound volume
        /// </summary>
        /// <param name="source">Audio source to play looping</param>
        /// <param name="volumeScale">Additional volume scale</param>
        /// <param name="fadeSeconds">Seconds to fade in and out</param>
        public static void PlayLoopingSound(AudioSource source, float volumeScale, float fadeSeconds)
        {
            PlayLooping(source, Sounds, volumeScale, fadeSeconds, false, false);
            UpdateSounds();
        }

        /// <summary>
        /// Play a music track once - music volume will be affected by the global music volume
        /// </summary>
        /// <param name="source"></param>
        /// <param name="clip"></param>
        public static void PlayOneShotMusic(AudioSource source, AudioClip clip)
        {
            PlayOneShotMusic(source, clip, 1.0f);
        }

        /// <summary>
        /// Play a music track once - music volume will be affected by the global music volume
        /// </summary>
        /// <param name="source">Audio source</param>
        /// <param name="clip">Clip</param>
        /// <param name="volumeScale">Additional volume scale</param>
        public static void PlayOneShotMusic(AudioSource source, AudioClip clip, float volumeScale)
        {
            EnsureCreated();

            int index = MusicOneShot.IndexOf(source);
            if (index >= 0)
            {
                MusicOneShot.RemoveAt(index);
            }

            source.PlayOneShot(clip, volumeScale * _musicVolume);
            MusicOneShot.Add(source);
        }

        /// <summary>
        /// Play a looping music track - music volume will be affected by the global music volume
        /// </summary>
        /// <param name="source">Audio source</param>
        public static void PlayLoopingMusic(AudioSource source)
        {
            PlayLoopingMusic(source, 1.0f, 1.0f, false);
        }

        /// <summary>
        /// Play a looping music track - music volume will be affected by the global music volume
        /// </summary>
        /// <param name="source">Audio source</param>
        /// <param name="volumeScale">Additional volume scale</param>
        /// <param name="fadeSeconds">Seconds to fade in and out</param>
        /// <param name="persist">Whether to persist the looping music between scene changes</param>
        public static void PlayLoopingMusic(AudioSource source, float volumeScale, float fadeSeconds, bool persist)
        {
            PlayLooping(source, Music, volumeScale, fadeSeconds, persist, true);
            UpdateMusic();
        }

        /// <summary>
        /// Stop looping a sound for an audio source
        /// </summary>
        /// <param name="source">Audio source to stop looping sound for</param>
        public static void StopLoopingSound(AudioSource source)
        {
            StopLooping(source, Sounds);
        }

        /// <summary>
        /// Stop looping music for an audio source
        /// </summary>
        /// <param name="source">Audio source to stop looping music for</param>
        public static void StopLoopingMusic(AudioSource source)
        {
            StopLooping(source, Music);
        }

        /// <summary>
        /// Stop all looping sounds, music, and music one shots. Non-looping sounds are not stopped.
        /// </summary>
        public static void StopAll()
        {
            StopAllLoopingSounds();
            StopNonLoopingSounds();
        }

        /// <summary>
        /// Stop all looping sounds and music. Music one shots and non-looping sounds are not stopped.
        /// </summary>
        public static void StopAllLoopingSounds()
        {
            foreach (LoopingAudioSource s in Sounds)
            {
                s.Stop();
            }

            foreach (LoopingAudioSource s in Music)
            {
                s.Stop();
            }
        }

        /// <summary>
        /// Stop all non-looping sounds. Looping sounds and looping music are not stopped.
        /// </summary>
        public static void StopNonLoopingSounds()
        {
            foreach (AudioSource s in MusicOneShot)
            {
                s.Stop();
            }
        }

        /// <summary>
        /// Pause all sounds
        /// </summary>
        public static void PauseAll()
        {
            foreach (LoopingAudioSource s in Sounds)
            {
                s.Pause();
            }

            foreach (LoopingAudioSource s in Music)
            {
                s.Pause();
            }
        }

        /// <summary>
        /// Unpause and resume all sounds
        /// </summary>
        public static void ResumeAll()
        {
            foreach (LoopingAudioSource s in Sounds)
            {
                s.Resume();
            }

            foreach (LoopingAudioSource s in Music)
            {
                s.Resume();
            }
        }

        /// <summary>
        /// Global music volume multiplier
        /// </summary>
        public static float MusicVolume
        {
            get => _musicVolume;
            set
            {
                if (Mathf.Approximately(value, _musicVolume))
                {
                    return;
                }

                _musicVolume = value;
                UpdateMusic();
            }
        }

        /// <summary>
        /// Global sound volume multiplier
        /// </summary>
        public static float SoundVolume
        {
            get => _soundVolume;
            set
            {
                if (!Mathf.Approximately(value, _soundVolume))
                {
                    return;
                }

                _soundVolume = value;
                UpdateSounds();
            }
        }

        /// <summary>
        /// Whether to pause sounds when the application is paused and resume them when the application is activated.
        /// Player option "Run In Background" must be selected to enable this. Default is true.
        /// </summary>
        public static bool PauseSoundsOnApplicationPause
        {
            get { return _pauseSoundsOnApplicationPause; }
            set { _pauseSoundsOnApplicationPause = value; }
        }
    }
}