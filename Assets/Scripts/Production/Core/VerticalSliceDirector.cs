using UnityEngine;

public class VerticalSliceDirector : MonoBehaviour
{
    public enum Beat { SpiritDiscovery, Shrine, Memory, Combat, Ability, Traversal, Boss, Complete }

    [SerializeField] private Beat currentBeat;
    [SerializeField] private GameObject[] beatRoots;

    public Beat CurrentBeat => currentBeat;

    private void Start() => ApplyBeat();

    public void Advance()
    {
        if (currentBeat == Beat.Complete) return;
        currentBeat++;
        ApplyBeat();
    }

    public void SetBeat(Beat beat)
    {
        currentBeat = beat;
        ApplyBeat();
    }

    private void ApplyBeat()
    {
        if (beatRoots == null) return;
        for (int i = 0; i < beatRoots.Length; i++)
            if (beatRoots[i] != null) beatRoots[i].SetActive(i <= (int)currentBeat);
    }
}
