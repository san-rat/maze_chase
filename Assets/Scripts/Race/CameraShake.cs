using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // Singleton
    public static CameraShake Instance;

    // Original camera position
    private Vector3 originalPosition;

    void Awake()
    {
        Instance = this;
        originalPosition = transform.localPosition;
    }

    // Call this to shake the camera
    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines();
        StartCoroutine(DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Random offset using Perlin noise
            float x = (Mathf.PerlinNoise(
                Time.time * 10f, 0f) - 0.5f) * magnitude;
            float y = (Mathf.PerlinNoise(
                0f, Time.time * 10f) - 0.5f) * magnitude;

            transform.localPosition = originalPosition +
                new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to original position
        transform.localPosition = originalPosition;
    }
}