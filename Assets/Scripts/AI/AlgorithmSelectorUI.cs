using UnityEngine;
using MazeChase.Race;

namespace MazeChase.AI
{
    public class AlgorithmSelectorUI : MonoBehaviour
    {
        [Header("UI Panel")]
        public GameObject selectorPanel;

        [Header("Reference")]
        public AIAgentController aiController; // Changed to match your previous script

        private void Start()
        {
            if (selectorPanel != null)
                selectorPanel.SetActive(true);

            if (aiController == null)
                aiController = FindAnyObjectByType<AIAgentController>();

            Debug.Log("Press 1 for UCS | Press 2 for BFS | Press 3 for A*");
        }

        private void Update()
        {
            // Press 1 for UCS (Index 0)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("1 pressed - UCS Selected!");
                if (aiController != null)
                    aiController.SetAlgorithm(0); 
                if (selectorPanel != null)
                    selectorPanel.SetActive(false);
            }

            // Press 2 for BFS (Index 1)
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("2 pressed - BFS Selected!");
                if (aiController != null)
                    aiController.SetAlgorithm(1);
                if (selectorPanel != null)
                    selectorPanel.SetActive(false);
            }

            // --- ADDED THIS FOR A* ---
            // Press 3 for A* (Index 2)
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("3 pressed - A* Selected!");
                if (aiController != null)
                    aiController.SetAlgorithm(2);
                if (selectorPanel != null)
                    selectorPanel.SetActive(false);
            }
        }
    }
}