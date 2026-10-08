using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class MainMenuPresentation : MonoBehaviour
{
    [SerializeField, Range(0.2f, 1.5f)] private float introDuration = 0.65f;
    [SerializeField] private RectTransform[] titleParts;
    [SerializeField] private RectTransform[] menuButtons;
    [SerializeField] private Transform[] backgroundLayers;
    [SerializeField, Range(0.85f, 1f)] private float titleStartScale = 0.92f;
    [SerializeField, Range(0f, 120f)] private float buttonStartOffset = 42f;
    [SerializeField, Range(0f, 15f)] private float backgroundDrift = 3f;

    private CanvasGroup group;
    private Coroutine intro;
    private Vector3[] titleBaseScales;
    private Vector2[] buttonOrigins;
    private Vector3[] backgroundOrigins;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();

        if (titleParts != null)
        {
            titleBaseScales = new Vector3[titleParts.Length];
            for (int i = 0; i < titleParts.Length; i++)
                if (titleParts[i] != null) titleBaseScales[i] = titleParts[i].localScale;
        }

        if (menuButtons != null)
        {
            buttonOrigins = new Vector2[menuButtons.Length];
            for (int i = 0; i < menuButtons.Length; i++)
                if (menuButtons[i] != null) buttonOrigins[i] = menuButtons[i].anchoredPosition;
        }

        if (backgroundLayers != null)
        {
            backgroundOrigins = new Vector3[backgroundLayers.Length];
            for (int i = 0; i < backgroundLayers.Length; i++)
                if (backgroundLayers[i] != null) backgroundOrigins[i] = backgroundLayers[i].localPosition;
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
            Transform layer = backgroundLayers[i];
            if (layer == null) continue;

            float depth = 1f + i * 0.45f;
            layer.localPosition = backgroundOrigins[i] + Vector3.right * (wave * backgroundDrift / depth);
        }
    }

    private IEnumerator PlayIntro()
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        if (titleParts != null && titleBaseScales != null)
        {
            for (int i = 0; i < titleParts.Length; i++)
                if (titleParts[i] != null) titleParts[i].localScale = titleBaseScales[i] * titleStartScale;
        }

        if (menuButtons != null && buttonOrigins != null)
        {
            for (int i = 0; i < menuButtons.Length; i++)
            {
                if (menuButtons[i] == null) continue;
                float stagger = buttonStartOffset * (1f + i * 0.2f);
                menuButtons[i].anchoredPosition = buttonOrigins[i] + Vector2.right * stagger;
            }
        }

        float elapsed = 0f;
        while (elapsed < introDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / introDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            group.alpha = eased;

            if (titleParts != null && titleBaseScales != null)
            {
                for (int i = 0; i < titleParts.Length; i++)
                {
                    if (titleParts[i] == null) continue;
                    titleParts[i].localScale = Vector3.LerpUnclamped(
                        titleBaseScales[i] * titleStartScale,
                        titleBaseScales[i],
                        eased);
                }
            }

            if (menuButtons != null && buttonOrigins != null)
            {
                for (int i = 0; i < menuButtons.Length; i++)
                {
                    if (menuButtons[i] == null) continue;
                    float stagger = buttonStartOffset * (1f + i * 0.2f);
                    Vector2 start = buttonOrigins[i] + Vector2.right * stagger;
                    menuButtons[i].anchoredPosition = Vector2.LerpUnclamped(start, buttonOrigins[i], eased);
                }
            }

            yield return null;
        }

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        if (titleParts != null && titleBaseScales != null)
            for (int i = 0; i < titleParts.Length; i++)
                if (titleParts[i] != null) titleParts[i].localScale = titleBaseScales[i];

        if (menuButtons != null && buttonOrigins != null)
            for (int i = 0; i < menuButtons.Length; i++)
                if (menuButtons[i] != null) menuButtons[i].anchoredPosition = buttonOrigins[i];

        intro = null;
    }
}
