using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class MainMenuPresentation : MonoBehaviour
{
    [SerializeField, Min(0.05f)] private float fadeDuration = 0.4f;
    [SerializeField] private RectTransform title;
    [SerializeField, Range(0.8f, 1f)] private float titleStartScale = 0.94f;

    private CanvasGroup group;
    private Coroutine intro;
    private Vector3 titleScale;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (title != null) titleScale = title.localScale;
    }

    private void OnEnable()
    {
        if (intro != null) StopCoroutine(intro);
        intro = StartCoroutine(PlayIntro());
    }

    private void OnDisable()
    {
        if (intro != null) StopCoroutine(intro);
        intro = null;
    }

    private IEnumerator PlayIntro()
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        if (title != null)
            title.localScale = titleScale * titleStartScale;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            group.alpha = eased;
            if (title != null)
                title.localScale = Vector3.LerpUnclamped(titleScale * titleStartScale, titleScale, eased);

            yield return null;
        }

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
        if (title != null) title.localScale = titleScale;
        intro = null;
    }
}
