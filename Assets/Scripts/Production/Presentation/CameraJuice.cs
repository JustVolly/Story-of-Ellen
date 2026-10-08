using System.Collections;
using UnityEngine;

public class CameraJuice : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float defaultShakeDuration = 0.12f;
    [SerializeField] private float defaultShakeStrength = 0.12f;
    private Coroutine shakeRoutine;

    public void Shake() => Shake(defaultShakeDuration, defaultShakeStrength);

    public void Shake(float duration, float strength)
    {
        if (cameraTransform == null) return;
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        Vector3 origin = cameraTransform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cameraTransform.localPosition = origin + (Vector3)(Random.insideUnitCircle * strength);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cameraTransform.localPosition = origin;
        shakeRoutine = null;
    }
}
