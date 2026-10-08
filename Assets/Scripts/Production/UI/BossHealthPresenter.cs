using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthPresenter : MonoBehaviour
{
    [SerializeField] private EnemyHealth bossHealth;
    [SerializeField] private BossController boss;
    [SerializeField] private GameObject root;
    [SerializeField] private Image fill;
    [SerializeField] private TextMeshProUGUI phaseText;

    private void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    private void OnEnable()
    {
        if (bossHealth != null) bossHealth.HealthChanged += OnHealthChanged;
        if (boss != null)
        {
            boss.PhaseChanged += OnPhaseChanged;
            boss.Defeated += OnDefeated;
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (bossHealth != null) bossHealth.HealthChanged -= OnHealthChanged;
        if (boss != null)
        {
            boss.PhaseChanged -= OnPhaseChanged;
            boss.Defeated -= OnDefeated;
        }
    }

    public void Show()
    {
        if (root != null) root.SetActive(true);
        Refresh();
    }

    private void Refresh()
    {
        if (bossHealth != null) OnHealthChanged(bossHealth.CurrentHealth, bossHealth.MaxHealth);
        if (boss != null) OnPhaseChanged(boss.CurrentPhase);
    }

    private void OnHealthChanged(int current, int max)
    {
        if (fill != null) fill.fillAmount = max <= 0 ? 0f : Mathf.Clamp01((float)current / max);
    }

    private void OnPhaseChanged(BossController.Phase phase)
    {
        if (phaseText != null) phaseText.text = "PHASE " + ((int)phase + 1);
    }

    private void OnDefeated()
    {
        if (root != null) root.SetActive(false);
    }
}
