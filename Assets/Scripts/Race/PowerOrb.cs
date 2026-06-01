using UnityEngine;
using MazeChase.Race;
using MazeChase.AI;

public class PowerOrb : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatSpeed = 1.5f;
    public float floatHeight = 0.3f;

    [Header("Rotation")]
    public float rotateSpeed = 90f;

    [Header("Pulse Glow")]
    public Renderer orbRenderer;
    public Color glowColorA = new Color(1f, 0.8f, 0f);
    public Color glowColorB = new Color(1f, 0.4f, 0f);
    public float pulseSpeed = 2f;

    [Header("Sound")]
    public AudioClip collectSound;
    private AudioSource audioSource;

    private float startY;
    private bool isGrabbed = false;
    private Transform player;

    void Start()
    {
        startY = transform.position.y;

        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        GameObject playerObj =
            GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (orbRenderer == null)
            orbRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (isGrabbed) return;

        // Float up and down
        float newY = startY + Mathf.Sin(
            Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z);

        // Rotate
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);

        // Pulse glow
        if (orbRenderer != null)
        {
            float t = (Mathf.Sin(
                Time.time * pulseSpeed) + 1f) / 2f;
            Color glow = Color.Lerp(
                glowColorA, glowColorB, t);
            orbRenderer.material.SetColor(
                "_EmissionColor", glow);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isGrabbed) return;

        bool isPlayer = other.CompareTag("Player") ||
                        other.name.Contains("Player") ||
                        other.name.Contains("Armature");

        if (isPlayer)
            GrabOrb();
    }

    void GrabOrb()
    {
        isGrabbed = true;
        Debug.Log("=== PowerOrb GRABBED ===");

        // Play collect sound at orb position
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(
                collectSound, transform.position, 2.5f);

        // Set AI back 10 nodes
        AIAgentController aiAgent =
            FindAnyObjectByType<AIAgentController>();

        if (aiAgent != null)
        {
            Debug.Log("Calling SetBackNodes...");
            aiAgent.SetBackNodes(10);
        }
        else
        {
            Debug.LogError("AIAgentController NOT FOUND!");
        }

        // Hide orb
        gameObject.SetActive(false);
    }
}