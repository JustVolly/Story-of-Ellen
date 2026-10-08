using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanelTransition : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private CanvasGroup group;
    [SerializeField, Range(0.05f, 1f)] private float duration = 0.22f;
    [SerializeField, Range(0.8f, 1f)] private float hiddenScale = 0.94f;
    [SerializeField] private bool animateOnEnable = true;

    private Coroutine routine;

    private void Awake()
    {
        if (panel == null) panel = transform as RectTransform;
        if (group == null) group = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (animateOnEnable) Show();
        else SetImmediate(true);
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        StartTransition(true);
    }

    public void Hide()
    {
        if (!gameObject.activeInHierarchy) return;
        StartTransition(false);
    }

    public void SetImmediate(bool visible)
    {
        if (group == null) return;

        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;

        if (panel != null)
            panel.localScale = Vector3.one * (visible ? 1f : hiddenScale);
    }

    private void StartTransition(bool visible)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Transition(visible));
    }

    private IEnumerator Transition(bool visible)
    {
        if (group == null) yield break;

        float startAlpha = group.alpha;
        float targetAlpha = visible ? 1f : 0f;
        Vector3 startScale = panel != null ? panel.localScale : Vector3.one;
        Vector3 targetScale = Vector3.one * (visible ? 1f : hiddenScale);

        group.interactable = false;
        group.blocksRaycasts = visible;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            if (panel != null) panel.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        group.alpha = targetAlpha;
        if (panel != null) panel.localScale = targetScale;
        group.interactable = visible;
        group.blocksRaycasts = visible;
        routine = null;

        if (!visible)
            gameObject.SetActive(false);
    }
}
