using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    private Coroutine routine;

    public void Play(float duration = 0.06f, float timeScale = 0.05f)
    {
        if (Time.timeScale <= 0f) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run(duration, timeScale));
    }

    private IEnumerator Run(float duration, float scale)
    {
        float previous = Time.timeScale;
        Time.timeScale = Mathf.Clamp(scale, 0.01f, 1f);
        yield return new WaitForSecondsRealtime(duration);
        if (Time.timeScale > 0f) Time.timeScale = previous;
        routine = null;
    }
}
