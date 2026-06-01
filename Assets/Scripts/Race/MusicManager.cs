using UnityEngine;

namespace MazeChase.Race
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance;

        [Header("Audio Tracks (all play simultaneously)")]
        public AudioClip track1;
        public AudioClip track2;
        public AudioClip track3;

        [Header("Individual Track Volumes")]
        [Range(0f, 1f)] public float track1Volume = 0.5f;
        [Range(0f, 1f)] public float track2Volume = 0.5f;
        [Range(0f, 1f)] public float track3Volume = 0.5f;

        // Separate AudioSource for each track
        private AudioSource source1;
        private AudioSource source2;
        private AudioSource source3;

        void Awake()
        {
            Instance = this;

            // Create 3 separate AudioSources
            source1 = gameObject.AddComponent<AudioSource>();
            source2 = gameObject.AddComponent<AudioSource>();
            source3 = gameObject.AddComponent<AudioSource>();
        }

        void Start()
        {
            // Setup and play all 3 simultaneously
            SetupSource(source1, track1, track1Volume);
            SetupSource(source2, track2, track2Volume);
            SetupSource(source3, track3, track3Volume);

            Debug.Log("MusicManager: All 3 tracks playing!");
        }

        void SetupSource(AudioSource source,
            AudioClip clip, float volume)
        {
            if (clip == null)
            {
                Debug.LogWarning(
                    "MusicManager: A track is not assigned!");
                return;
            }

            source.clip = clip;
            source.volume = volume;
            source.loop = true;      // ← loop forever
            source.playOnAwake = false;
            source.Play();
        }

        public void Pause()
        {
            source1?.Pause();
            source2?.Pause();
            source3?.Pause();
            Debug.Log("Music paused");
        }

        public void Resume()
        {
            source1?.UnPause();
            source2?.UnPause();
            source3?.UnPause();
            Debug.Log("Music resumed");
        }

        // Call these to adjust volume live
        public void SetTrack1Volume(float v)
        {
            track1Volume = v;
            if (source1 != null) source1.volume = v;
        }

        public void SetTrack2Volume(float v)
        {
            track2Volume = v;
            if (source2 != null) source2.volume = v;
        }

        public void SetTrack3Volume(float v)
        {
            track3Volume = v;
            if (source3 != null) source3.volume = v;
        }

        // Mute/unmute individual tracks
        public void MuteTrack1(bool mute)
        {
            if (source1 != null) source1.mute = mute;
        }

        public void MuteTrack2(bool mute)
        {
            if (source2 != null) source2.mute = mute;
        }

        public void MuteTrack3(bool mute)
        {
            if (source3 != null) source3.mute = mute;
        }
    }
}