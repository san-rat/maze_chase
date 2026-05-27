using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    void Awake()
    {
        Instance = this;
    }

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
            // Apply shake directly to camera
            float x = Random.Range(
                -magnitude, magnitude);
            float y = Random.Range(
                -magnitude, magnitude);

            // Apply as world position offset
            Camera.main.transform.localPosition +=
                new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            // Wait one frame
            yield return new WaitForEndOfFrame();

            // Reset position each frame
            Camera.main.transform.localPosition -=
                new Vector3(x, y, 0);
        }
    }
}