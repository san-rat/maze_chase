using UnityEngine;

public class BarrierController : MonoBehaviour
{
    [Header("Barrier Settings")]
    public float raisedHeight = 2.5f;
    public float riseSpeed = 3f;
    public float activationDistance = 3f;

    [Header("State")]
    public bool isRaised = false;

    private bool isActivated = false;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Transform player;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition;

        GameObject playerObj =
            GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // Smooth movement
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            riseSpeed * Time.deltaTime
        );

        // Check player proximity and E key
        if (player != null && !isActivated)
        {
            float distance = Vector3.Distance(
                transform.position,
                player.position
            );

            if (distance <= activationDistance &&
                Input.GetKeyDown(KeyCode.E))
            {
                ActivateBarrier();
            }
        }
    }

    public void ActivateBarrier()
    {
        if (isActivated) return;
        isActivated = true;
        isRaised = true;
        targetPosition = startPosition +
            new Vector3(0, raisedHeight, 0);
        Debug.Log("Barrier raised: " + gameObject.name);
    }
}