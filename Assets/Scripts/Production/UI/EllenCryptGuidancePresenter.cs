using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Level 3-only unobtrusive world narrative. Uses unscaled time so a pause or
/// hit-stop cannot strand an on-screen instructional cue indefinitely.
/// </summary>
public sealed class EllenCryptGuidancePresenter : MonoBehaviour
{
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField, Min(0.5f)] private float holdSeconds = 3.5f;
    [SerializeField, Min(0.05f)] private float fadeSeconds = 0.25f;

    private Coroutine displayRoutine;
    public bool IsConfigured => panel != null && message != null;

    private void Awake()
    {
        if (panel != null)
        {
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }
    }

    public void Show(string text)
    {
        if (!isActiveAndEnabled || !IsConfigured || string.IsNullOrWhiteSpace(text))
            return;

        if (displayRoutine != null) StopCoroutine(displayRoutine);
        message.text = text;
        displayRoutine = StartCoroutine(Display());
    }

    private IEnumerator Display()
    {
        yield return FadeTo(1f);
        yield return new WaitForSecondsRealtime(holdSeconds);
        yield return FadeTo(0f);
        displayRoutine = null;
    }

    private IEnumerator FadeTo(float target)
    {
        while (!Mathf.Approximately(panel.alpha, target))
        {
            panel.alpha = Mathf.MoveTowards(panel.alpha, target,
                Time.unscaledDeltaTime / Mathf.Max(0.05f, fadeSeconds));
            yield return null;
        }
        panel.alpha = target;
    }

    private void OnDisable()
    {
        if (displayRoutine != null) StopCoroutine(displayRoutine);
        displayRoutine = null;
        if (panel != null) panel.alpha = 0f;
    }
}
