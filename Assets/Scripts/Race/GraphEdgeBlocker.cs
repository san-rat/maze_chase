using UnityEngine;
using MazeChase.Race;

public class GraphEdgeBlocker : MonoBehaviour
{
    [Header("Barrier Node Positions")]
    // Drag two GraphNode objects here in Inspector
    public Transform nodeA;
    public Transform nodeB;

    // Reference to barrier on this object
    private BarrierController barrier;
    private bool lastRaisedState = false;

    void Start()
    {
        barrier = GetComponent<BarrierController>();

        if (nodeA == null || nodeB == null)
            Debug.LogWarning(gameObject.name +
                ": GraphEdgeBlocker missing node references!");
    }

    void Update()
    {
        if (barrier == null) return;

        // Only act when barrier state changes
        if (barrier.isRaised != lastRaisedState)
        {
            lastRaisedState = barrier.isRaised;

            if (DynamicGraphUpdater.Instance == null)
            {
                Debug.LogWarning("DynamicGraphUpdater " +
                    "instance not found!");
                return;
            }

            if (barrier.isRaised)
            {
                // Barrier rose — block the edge
                DynamicGraphUpdater.Instance.BlockEdge(
                    nodeA.position,
                    nodeB.position
                );
            }
            else
            {
                // Barrier lowered — restore edge
                DynamicGraphUpdater.Instance.UnblockEdge(
                    nodeA.position,
                    nodeB.position
                );
            }
        }
    }
}