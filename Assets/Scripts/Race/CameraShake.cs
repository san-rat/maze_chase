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
        float originalY = transform.localPosition.y;

        while (elapsed < duration)
        {
            float x = Random.Range(
                -magnitude, magnitude);
            float y = Random.Range(
                -magnitude, magnitude);

            transform.localPosition += 
                new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();

            // Reset each frame
            transform.localPosition = new Vector3(
                0, originalY, 0);
        }
    }
}