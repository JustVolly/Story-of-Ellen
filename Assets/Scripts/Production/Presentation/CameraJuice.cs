using System.Collections;
using UnityEngine;

public class CameraJuice : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float defaultShakeDuration = 0.12f;
    [SerializeField] private float defaultShakeStrength = 0.12f;

    private Coroutine shakeRoutine;
    private Vector3 shakeOrigin;

    public void Shake() => Shake(defaultShakeDuration, defaultShakeStrength);

    public void Shake(float duration, float strength)
    {
        if (cameraTransform == null) return;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            cameraTransform.localPosition = shakeOrigin;
        }

        shakeOrigin = cameraTransform.localPosition;
        shakeRoutine = StartCoroutine(ShakeRoutine(Mathf.Max(0f, duration), Mathf.Max(0f, strength)));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cameraTransform.localPosition = shakeOrigin + (Vector3)(Random.insideUnitCircle * strength);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        cameraTransform.localPosition = shakeOrigin;
        shakeRoutine = null;
    }

    private void OnDisable()
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = null;
        if (cameraTransform != null) cameraTransform.localPosition = shakeOrigin;
    }
}
