using UnityEngine;
using MazeChase.Race;

namespace MazeChase.AI
{
    public class AlgorithmSelectorUI : MonoBehaviour
    {
        [Header("UI Panel")]
        public GameObject selectorPanel;

        [Header("Reference")]
        public AIRaceController aiController;

        private void Start()
        {
            if (selectorPanel != null)
                selectorPanel.SetActive(true);

            if (aiController == null)
                aiController = FindAnyObjectByType<AIRaceController>();

            Debug.Log("Press 1 for UCS | Press 2 for BFS");
        }

        private void Update()
        {
            // Press 1 for UCS
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("1 pressed - UCS Selected!");
                if (aiController != null)
                    aiController.SetAlgorithm(true);
                if (selectorPanel != null)
                    selectorPanel.SetActive(false);
            }

            // Press 2 for BFS
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("2 pressed - BFS Selected!");
                if (aiController != null)
                    aiController.SetAlgorithm(false);
                if (selectorPanel != null)
                    selectorPanel.SetActive(false);
            }
        }
    }
}