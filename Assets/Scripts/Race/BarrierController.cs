using UnityEngine;
using MazeChase.Race;

public class BarrierController : MonoBehaviour
{
    [Header("Barrier Settings")]
    public float raisedHeight = 1.2f;
    public float riseSpeed = 3f;
    public float activationDistance = 3f;

    [Header("Glow Settings")]
    public Color normalColor = Color.grey;
    public Color glowColor = Color.red;

    public bool isRaised = false;

    private Vector3 loweredPosition;
    private Vector3 raisedPosition;
    private Vector3 targetPosition;

    private Transform player;
    private Renderer wallRenderer;
    private bool isGlowing = false;
    private float toggleCooldown = 0f;

    [Header("Audio")]
    public AudioClip activationSound;
    private AudioSource audioSource;

    void Start()
    {
        Debug.Log("BarrierCountUI at Start: " + BarrierCountUI.Instance);

        loweredPosition = transform.position;
        raisedPosition = loweredPosition +
            new Vector3(0, raisedHeight, 0);
        targetPosition = loweredPosition;

        wallRenderer = GetComponent<Renderer>();
        if (wallRenderer != null)
            wallRenderer.material.color = normalColor;

        GameObject playerObj =
            GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            riseSpeed * Time.deltaTime
        );

        if (toggleCooldown > 0)
            toggleCooldown -= Time.deltaTime;

        if (player == null) return;

        float distance = Vector3.Distance(
            transform.position, player.position);

        if (distance <= activationDistance && !isGlowing)
        {
            isGlowing = true;
            if (wallRenderer != null)
                wallRenderer.material.color = glowColor;
        }
        else if (distance > activationDistance && isGlowing)
        {
            isGlowing = false;
            if (wallRenderer != null)
                wallRenderer.material.color = normalColor;
        }

        if (distance <= activationDistance &&
            Input.GetKeyDown(KeyCode.E) &&
            toggleCooldown <= 0)
        {
            ToggleBarrier();
        }
    }

    public void ToggleBarrier()
    {
        if (activationSound != null)
            audioSource.PlayOneShot(activationSound);

        toggleCooldown = 0.5f;
        isRaised = !isRaised;

        if (isRaised)
        {
            targetPosition = raisedPosition;
            Debug.Log("Barrier raised: " + gameObject.name);

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(0.3f, 0.2f);

            if (BarrierCountUI.Instance != null)
                BarrierCountUI.Instance.BarrierUsed();
        }
        else
        {
            targetPosition = loweredPosition;
            Debug.Log("Barrier lowered: " + gameObject.name);

            if (BarrierCountUI.Instance != null)
                BarrierCountUI.Instance.BarrierRestored();
        }
        Debug.Log("BarrierCountUI check: " + BarrierCountUI.Instance);
    }

    public void ActivateBarrier()
    {
        ToggleBarrier();
    }
}