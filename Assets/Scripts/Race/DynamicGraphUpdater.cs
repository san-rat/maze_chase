using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MazeChase.AI;

namespace MazeChase.Race
{
    public class DynamicGraphUpdater : MonoBehaviour
    {
        [Header("UI Reference")]
        public GameObject recalculatingUI;  // ← MOVED INSIDE class

        private const float NodeMatchTolerance = 3f;

        public static DynamicGraphUpdater Instance;
        private AIAgentController aiAgent;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            aiAgent = FindAnyObjectByType<AIAgentController>();

            if (aiAgent == null)
                Debug.LogWarning("DynamicGraphUpdater: AIAgentController not found!");
            else
                Debug.Log("DynamicGraphUpdater: Connected to AIAgentController!");
        }

        public void BlockEdge(Vector3 posA, Vector3 posB)
        {
            if (aiAgent == null || aiAgent.adjacency == null)
            {
                Debug.LogWarning("Cannot block edge — no adjacency data found!");
                return;
            }

            if (!TryResolveGraphNode(posA, out Vector3 keyA) ||
                !TryResolveGraphNode(posB, out Vector3 keyB))
            {
                Debug.LogWarning("Cannot block edge - barrier nodes do not match graph nodes.");
                return;
            }

            bool removed = false;

            if (aiAgent.adjacency.ContainsKey(keyA))
            {
                removed |= aiAgent.adjacency[keyA].RemoveAll(
                    n => Vector3.Distance(n.Item1, keyB) < NodeMatchTolerance
                ) > 0;
            }

            if (aiAgent.adjacency.ContainsKey(keyB))
            {
                removed |= aiAgent.adjacency[keyB].RemoveAll(
                    n => Vector3.Distance(n.Item1, keyA) < NodeMatchTolerance
                ) > 0;
            }

            if (removed)
            {
                Debug.Log("IS: Edge blocked between " + keyA + " and " + keyB);
                NotifyRecalculation();
            }
        }

        public void UnblockEdge(Vector3 posA, Vector3 posB)
        {
            if (aiAgent == null || aiAgent.adjacency == null)
                return;

            if (!TryResolveGraphNode(posA, out Vector3 keyA) ||
                !TryResolveGraphNode(posB, out Vector3 keyB))
            {
                Debug.LogWarning("Cannot restore edge - barrier nodes do not match graph nodes.");
                return;
            }

            float cost = Vector3.Distance(keyA, keyB);

            if (aiAgent.adjacency.ContainsKey(keyA) && !HasNeighbor(keyA, keyB))
                aiAgent.adjacency[keyA].Add((keyB, cost));

            if (aiAgent.adjacency.ContainsKey(keyB) && !HasNeighbor(keyB, keyA))
                aiAgent.adjacency[keyB].Add((keyA, cost));

            Debug.Log("IS: Edge restored between " + keyA + " and " + keyB);
            NotifyRecalculation();
        }

        private float lastNotifyTime = -10f;

        public void NotifyRecalculation()
        {
            // Only show message every 5 seconds
            if (Time.time - lastNotifyTime < 5f) 
            {
                // Still recalculate but don't show UI
                if (aiAgent == null) return;
                aiAgent.StopAllCoroutines();
                aiAgent.StartCoroutine(aiAgent.RecalculatePath());
                return;
            }

            lastNotifyTime = Time.time;
            Debug.Log("IS: Path recalculation triggered!");

            if (aiAgent == null) return;
            aiAgent.StopAllCoroutines();
            aiAgent.StartCoroutine(aiAgent.RecalculatePath());

            // if (recalculatingUI != null)
            //     StartCoroutine(ShowRecalculatingMessage());
        }

        IEnumerator ShowRecalculatingMessage()
        {
            recalculatingUI.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            recalculatingUI.SetActive(false);
        }

        private bool TryResolveGraphNode(Vector3 position, out Vector3 key)
        {
            key = default;
            if (aiAgent?.adjacency == null) return false;

            float bestDistance = float.MaxValue;
            foreach (Vector3 candidate in aiAgent.adjacency.Keys)
            {
                float distance = Vector3.Distance(position, candidate);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    key = candidate;
                }
            }

            return bestDistance <= NodeMatchTolerance;
        }

        private bool HasNeighbor(Vector3 from, Vector3 to)
        {
            if (!aiAgent.adjacency.TryGetValue(from, out var neighbors))
                return false;

            return neighbors.Exists(
                n => Vector3.Distance(n.Item1, to) < NodeMatchTolerance);
        }
    }
}