using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    private Coroutine routine;
    private float restoreScale = 1f;

    public void Play(float duration = 0.06f, float timeScale = 0.05f)
    {
        if (Time.timeScale <= 0f) return;

        if (routine == null)
            restoreScale = Time.timeScale;
        else
            StopCoroutine(routine);

        Time.timeScale = Mathf.Clamp(timeScale, 0.01f, 1f);
        routine = StartCoroutine(Run(Mathf.Max(0f, duration)));
    }

    private IEnumerator Run(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (Time.timeScale > 0f) Time.timeScale = restoreScale;
        routine = null;
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        if (Time.timeScale > 0f && Time.timeScale < restoreScale)
            Time.timeScale = restoreScale;
    }
}
