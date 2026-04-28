using UnityEngine;
using UnityEngine.AI;

public class NavMeshTestMover : MonoBehaviour
{
    [SerializeField] private Transform target;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("NavMeshTestMover: Target is not assigned.");
            return;
        }

        if (agent == null)
        {
            Debug.LogError("NavMeshTestMover: NavMeshAgent component is missing.");
            return;
        }

        agent.SetDestination(target.position);
    }
}

