using UnityEngine;
using TMPro;

namespace MazeChase.Race    // ← ADD THIS
{
    public class RaceTimer : MonoBehaviour
    {
        [Header("UI Reference")]
        public TextMeshProUGUI timerText;

        private bool isRunning = false;
        private float elapsedTime = 0f;

        public static RaceTimer Instance;

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (!isRunning) return;

            elapsedTime += Time.deltaTime;

            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public void StartTimer()
        {
            isRunning = true;
            elapsedTime = 0f;
        }

        public void StopTimer()
        {
            isRunning = false;
        }

        public string GetTimeString()
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}                           // ← CLOSE THIS