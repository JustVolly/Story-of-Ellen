using System.Collections;
using TMPro;
using UnityEngine;

public class SpiritTutorialPresenter : MonoBehaviour
{
    [SerializeField] private SpiritWorldController spiritWorld;
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField, Min(0.1f)] private float duration = 3f;
    private bool shown;

    private void OnEnable()
    {
        if (spiritWorld != null) spiritWorld.WorldChanged += OnWorldChanged;
        if (panel != null) panel.alpha = 0f;
    }

    private void OnDisable()
    {
        if (spiritWorld != null) spiritWorld.WorldChanged -= OnWorldChanged;
    }

    private void OnWorldChanged(bool spirit)
    {
        if (!spirit || shown) return;
        shown = true;
        StartCoroutine(Show());
    }

    private IEnumerator Show()
    {
        if (message != null) message.text = "Spirit World reveals hidden paths, but staying here consumes energy.";
        if (panel != null) panel.alpha = 1f;
        yield return new WaitForSecondsRealtime(duration);
        if (panel != null) panel.alpha = 0f;
    }
}
