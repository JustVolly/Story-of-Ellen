using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class MainMenuPresentation : MonoBehaviour
{
    [SerializeField, Range(0.2f, 1.5f)] private float introDuration = 0.65f;
    [SerializeField] private RectTransform title;
    [SerializeField] private RectTransform playButton;
    [SerializeField] private RectTransform quitButton;
    [SerializeField] private RectTransform[] backgroundLayers;
    [SerializeField, Range(0.85f, 1f)] private float titleStartScale = 0.92f;
    [SerializeField, Range(0f, 120f)] private float buttonStartOffset = 42f;
    [SerializeField, Range(0f, 15f)] private float backgroundDrift = 3f;

    private CanvasGroup group;
    private Coroutine intro;
    private Vector3 titleBaseScale = Vector3.one;
    private Vector2 playOrigin;
    private Vector2 quitOrigin;
    private Vector2[] backgroundOrigins;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();

        if (title != null) titleBaseScale = title.localScale;
        if (playButton != null) playOrigin = playButton.anchoredPosition;
        if (quitButton != null) quitOrigin = quitButton.anchoredPosition;

        if (backgroundLayers != null)
        {
            backgroundOrigins = new Vector2[backgroundLayers.Length];
            for (int i = 0; i < backgroundLayers.Length; i++)
                if (backgroundLayers[i] != null) backgroundOrigins[i] = backgroundLayers[i].anchoredPosition;
        }
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

    private void Update()
    {
        if (backgroundLayers == null || backgroundOrigins == null || backgroundDrift <= 0f) return;

        float wave = Mathf.Sin(Time.unscaledTime * 0.22f);
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            RectTransform layer = backgroundLayers[i];
            if (layer == null) continue;

            float depth = 1f + i * 0.45f;
            layer.anchoredPosition = backgroundOrigins[i] + new Vector2(wave * backgroundDrift / depth, 0f);
        }
    }

    private IEnumerator PlayIntro()
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        if (title != null) title.localScale = titleBaseScale * titleStartScale;
        if (playButton != null) playButton.anchoredPosition = playOrigin + Vector2.right * buttonStartOffset;
        if (quitButton != null) quitButton.anchoredPosition = quitOrigin + Vector2.right * (buttonStartOffset * 1.25f);

        float elapsed = 0f;
        while (elapsed < introDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / introDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            group.alpha = eased;

            if (title != null)
                title.localScale = Vector3.LerpUnclamped(titleBaseScale * titleStartScale, titleBaseScale, eased);

            if (playButton != null)
                playButton.anchoredPosition = Vector2.LerpUnclamped(playOrigin + Vector2.right * buttonStartOffset, playOrigin, eased);

            if (quitButton != null)
                quitButton.anchoredPosition = Vector2.LerpUnclamped(quitOrigin + Vector2.right * (buttonStartOffset * 1.25f), quitOrigin, eased);

            yield return null;
        }

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        if (title != null) title.localScale = titleBaseScale;
        if (playButton != null) playButton.anchoredPosition = playOrigin;
        if (quitButton != null) quitButton.anchoredPosition = quitOrigin;

        intro = null;
    }
}
