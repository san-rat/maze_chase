using UnityEngine;
using System.Collections;

namespace MazeChase.Race
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance;

        [Header("Audio Tracks")]
        public AudioClip track1;
        public AudioClip track2;
        public AudioClip track3;

        [Header("Individual Track Volumes")]
        [Range(0f, 1f)] public float track1Volume = 0.5f;
        [Range(0f, 1f)] public float track2Volume = 0.5f;
        [Range(0f, 1f)] public float track3Volume = 0.5f;

        [Header("Track 3 Delay & Fade")]
        public float track3Delay = 3f;    // wait for countdown
        public float track3FadeIn = 4f;   // fade in duration

        private AudioSource source1;
        private AudioSource source2;
        private AudioSource source3;

        void Awake()
        {
            Instance = this;
            source1 = gameObject.AddComponent<AudioSource>();
            source2 = gameObject.AddComponent<AudioSource>();
            source3 = gameObject.AddComponent<AudioSource>();
        }

        void Start()
        {
            // Play track 1 and 2 immediately
            SetupSource(source1, track1, track1Volume);
            SetupSource(source2, track2, track2Volume);

            // Track 3 starts silent then fades in after delay
            if (track3 != null)
            {
                source3.clip = track3;
                source3.volume = 0f;
                source3.loop = true;
                source3.playOnAwake = false;
                source3.Play();
                StartCoroutine(FadeInTrack3());
            }
        }

        IEnumerator FadeInTrack3()
        {
            // Wait for countdown to finish
            yield return new WaitForSeconds(track3Delay);

            float elapsed = 0f;
            while (elapsed < track3FadeIn)
            {
                elapsed += Time.deltaTime;
                source3.volume = Mathf.Lerp(
                    0f, track3Volume,
                    elapsed / track3FadeIn);
                yield return null;
            }

            source3.volume = track3Volume;
            Debug.Log("Track 3 fully faded in!");
        }

        void SetupSource(AudioSource source,
            AudioClip clip, float volume)
        {
            if (clip == null) return;
            source.clip = clip;
            source.volume = volume;
            source.loop = true;
            source.playOnAwake = false;
            source.Play();
        }

        public void Pause()
        {
            source1?.Pause();
            source2?.Pause();
            source3?.Pause();
        }

        public void Resume()
        {
            source1?.UnPause();
            source2?.UnPause();
            source3?.UnPause();
        }

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
    }
}