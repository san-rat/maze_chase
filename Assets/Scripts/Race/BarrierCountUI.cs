using UnityEngine;
using TMPro;

namespace MazeChase.Race
{
    public class BarrierCountUI : MonoBehaviour
    {
        public TextMeshProUGUI countText;

        private int totalBarriers = 4;
        private int remainingBarriers = 4;

        public static BarrierCountUI Instance;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            UpdateUI();
        }

        public void BarrierUsed()
        {
            remainingBarriers--;
            if (remainingBarriers < 0)
                remainingBarriers = 0;
            UpdateUI();
        }

        public void BarrierRestored()
        {
            remainingBarriers++;
            if (remainingBarriers > totalBarriers)
                remainingBarriers = totalBarriers;
            UpdateUI();
        }

        void UpdateUI()
        {
            if (countText != null)
                countText.text = "Barriers: " +
                    remainingBarriers + "/" + totalBarriers;
        }
    }
}