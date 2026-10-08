using System.Collections;
using UnityEngine;

public class LevelUp : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] FireWorks;
    [SerializeField] private GameObject SetWinPanel;
    [SerializeField] private Rigidbody2D PlayerRigid;
    [SerializeField] private Animator PlayerAnim;
    [SerializeField, Min(0f)] private float finishHopVelocity = 8.5f;
    [SerializeField, Min(0f)] private float resultDelay = 1.25f;

    public float Forcing = 1400f;
    public bool isSetWin;
    public bool isFinish;

    private PlayerMovement playerMovement;
    private LevelFlowController levelFlow;
    private bool completionStarted;

    private void Awake()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        levelFlow = FindObjectOfType<LevelFlowController>();

        if (PlayerRigid == null && playerMovement != null)
            PlayerRigid = playerMovement.GetComponent<Rigidbody2D>();

        if (PlayerAnim == null && playerMovement != null)
            PlayerAnim = playerMovement.GetComponent<Animator>();
    }

    private void Start()
    {
        if (SetWinPanel != null) SetWinPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completionStarted || !other.CompareTag("Player")) return;

        completionStarted = true;
        isSetWin = true;
        isFinish = true;

        if (PlayerRigid != null)
            PlayerRigid.linearVelocity = new Vector2(PlayerRigid.linearVelocity.x, finishHopVelocity);

        if (PlayerAnim != null)
        {
            PlayerAnim.SetBool("run", false);
            PlayerAnim.SetBool("idle", false);
            PlayerAnim.SetBool("jump", true);
        }

        PlayFireworks();
        levelFlow?.TryComplete();
        StartCoroutine(ShowResults());
    }

    private IEnumerator ShowResults()
    {
        yield return new WaitForSecondsRealtime(resultDelay);

        if (SetWinPanel != null) SetWinPanel.SetActive(true);
        StopFireworks();

        if (PlayerAnim != null)
        {
            PlayerAnim.SetBool("jump", false);
            PlayerAnim.SetBool("idle", true);
        }

        isSetWin = false;
    }

    private void PlayFireworks()
    {
        if (FireWorks == null) return;
        foreach (ParticleSystem firework in FireWorks)
            if (firework != null) firework.Play();
    }

    private void StopFireworks()
    {
        if (FireWorks == null) return;
        foreach (ParticleSystem firework in FireWorks)
            if (firework != null) firework.Stop();
    }
}
