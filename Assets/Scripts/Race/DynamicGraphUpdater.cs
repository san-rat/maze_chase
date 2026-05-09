using System.Collections.Generic;
using UnityEngine;
using MazeChase.AI;

namespace MazeChase.Race
{
    public class DynamicGraphUpdater : MonoBehaviour
    {
        // Singleton — access from anywhere
        public static DynamicGraphUpdater Instance;

        // Reference to AI agent controller
        private AIAgentController aiAgent;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // Find the AI agent in the scene
            aiAgent = FindAnyObjectByType<AIAgentController>();

            if (aiAgent == null)
                Debug.LogWarning(
                    "DynamicGraphUpdater: " +
                    "AIAgentController not found!");
            else
                Debug.Log(
                    "DynamicGraphUpdater: " +
                    "Connected to AIAgentController!");
        }

        // Block edge between two world positions
        public void BlockEdge(Vector3 posA, Vector3 posB)
        {
            if (aiAgent == null || aiAgent.adjacency == null)
            {
                Debug.LogWarning("Cannot block edge — " +
                    "no adjacency data found!");
                return;
            }

            bool removed = false;

            // Remove posB from posA's neighbor list
            if (aiAgent.adjacency.ContainsKey(posA))
            {
                aiAgent.adjacency[posA].RemoveAll(
                    n => Vector3.Distance(n.Item1, posB) < 0.5f
                );
                removed = true;
            }

            // Remove posA from posB's neighbor list
            if (aiAgent.adjacency.ContainsKey(posB))
            {
                aiAgent.adjacency[posB].RemoveAll(
                    n => Vector3.Distance(n.Item1, posA) < 0.5f
                );
                removed = true;
            }

            if (removed)
            {
                Debug.Log("IS: Edge blocked between " +
                    posA + " and " + posB);
                NotifyRecalculation();
            }
        }

        // Restore edge between two world positions
        public void UnblockEdge(Vector3 posA, Vector3 posB)
        {
            if (aiAgent == null || aiAgent.adjacency == null)
                return;

            float cost = Vector3.Distance(posA, posB);

            // Add posB back to posA's neighbors
            if (aiAgent.adjacency.ContainsKey(posA))
            {
                aiAgent.adjacency[posA].Add((posB, cost));
            }

            // Add posA back to posB's neighbors
            if (aiAgent.adjacency.ContainsKey(posB))
            {
                aiAgent.adjacency[posB].Add((posA, cost));
            }

            Debug.Log("IS: Edge restored between " +
                posA + " and " + posB);

            NotifyRecalculation();
        }

        // Tell AI to recalculate its path
        public void NotifyRecalculation()
        {
            Debug.Log("IS: Path recalculation triggered!");

            if (aiAgent == null) return;

            // Stop current movement
            aiAgent.StopAllCoroutines();

            // Restart pathfinding with updated graph
            aiAgent.StartCoroutine(
                aiAgent.RecalculatePath()
            );
        }
    }
}