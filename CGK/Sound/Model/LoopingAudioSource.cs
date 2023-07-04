using UnityEngine;

namespace CGK.Sound.Model
{
 /// <summary>
    /// Provides an easy wrapper to looping audio sources with nice transitions for volume when starting and stopping
    /// </summary>
    public class LoopingAudioSource
    {
        /// <summary>
        /// The audio source that is looping
        /// </summary>
        public AudioSource AudioSource { get; private set; }

        /// <summary>
        /// The target volume
        /// </summary>
        public float TargetVolume { get; set; }

        /// <summary>
        /// The original target volume - useful if the global sound volume changes you can still have the original target volume to multiply by.
        /// </summary>
        public float OriginalTargetVolume { get; private set; }

        /// <summary>
        /// Is this sound stopping?
        /// </summary>
        public bool Stopping { get; private set; }

        /// <summary>
        /// Whether the looping audio source persists in between scene changes
        /// </summary>
        public bool Persist { get; private set; }

        /// <summary>
        /// Tag for the looping audio source
        /// </summary>
        public int Tag { get; set; }

        private float _startVolume;
        private float _startMultiplier;
        private float _stopMultiplier;
        private float _currentMultiplier;
        private float _timestamp;
        private bool _paused;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="audioSource">Audio source, will be looped automatically</param>
        /// <param name="startMultiplier">Start multiplier - seconds to reach peak sound</param>
        /// <param name="stopMultiplier">Stop multiplier - seconds to fade sound back to 0 volume when stopped</param>
        /// <param name="persist">Whether to persist the looping audio source between scene changes</param>
        public LoopingAudioSource(AudioSource audioSource, float startMultiplier, float stopMultiplier, bool persist)
        {
            AudioSource = audioSource;
            if (audioSource != null)
            {
                AudioSource.loop = true;
                AudioSource.volume = 0.0f;
                AudioSource.Stop();
            }

            this._startMultiplier = _currentMultiplier = startMultiplier;
            this._stopMultiplier = stopMultiplier;
            Persist = persist;
        }

        /// <summary>
        /// Play this looping audio source
        /// </summary>
        /// <param name="isMusic">True if music, false if sound effect</param>
        public void Play(bool isMusic)
        {
            Play(1.0f, isMusic);
        }

        /// <summary>
        /// Play this looping audio source
        /// </summary>
        /// <param name="targetVolume">Max volume</param>
        /// <param name="isMusic">True if music, false if sound effect</param>
        /// <returns>True if played, false if already playing or error</returns>
        public bool Play(float targetVolume, bool isMusic)
        {
            if (AudioSource != null)
            {
                AudioSource.volume = _startVolume = (AudioSource.isPlaying ? AudioSource.volume : 0.0f);
                AudioSource.loop = true;
                _currentMultiplier = _startMultiplier;
                OriginalTargetVolume = targetVolume;
                TargetVolume = targetVolume;
                Stopping = false;
                _timestamp = 0.0f;
                if (!AudioSource.isPlaying)
                {
                    AudioSource.Play();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Stop this looping audio source. The sound will fade out smoothly.
        /// </summary>
        public void Stop()
        {
            if (AudioSource != null && AudioSource.isPlaying && !Stopping)
            {
                _startVolume = AudioSource.volume;
                TargetVolume = 0.0f;
                _currentMultiplier = _stopMultiplier;
                Stopping = true;
                _timestamp = 0.0f;
            }
        }

        /// <summary>
        /// Pauses the looping audio source
        /// </summary>
        public void Pause()
        {
            if (AudioSource != null && !_paused && AudioSource.isPlaying)
            {
                _paused = true;
                AudioSource.Pause();
            }
        }

        /// <summary>
        /// Resumes the looping audio source
        /// </summary>
        public void Resume()
        {
            if (AudioSource != null && _paused)
            {
                _paused = false;
                AudioSource.UnPause();
            }
        }

        /// <summary>
        /// Update this looping audio source
        /// </summary>
        /// <returns>True if finished playing, false otherwise</returns>
        public bool Update()
        {
            if (AudioSource != null && AudioSource.isPlaying)
            {
                if ((AudioSource.volume = Mathf.Lerp(_startVolume, TargetVolume, (_timestamp += Time.deltaTime) / _currentMultiplier)) == 0.0f && Stopping)
                {
                    AudioSource.Stop();
                    Stopping = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return !_paused;
        }
    }
}