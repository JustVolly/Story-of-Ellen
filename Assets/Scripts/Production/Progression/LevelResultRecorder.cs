using UnityEngine;

public class LevelResultRecorder : MonoBehaviour
{
    [SerializeField] private LevelFlowController flow;

    private void OnEnable()
    {
        if (flow != null) flow.LevelCompleted += Record;
    }

    private void OnDisable()
    {
        if (flow != null) flow.LevelCompleted -= Record;
    }

    private void Record(LevelResult result)
    {
        ProgressionSave.Data data = ProgressionSave.Load();
        data.totalMemoryFragments = Mathf.Max(data.totalMemoryFragments, result.MemoriesFound);
        data.totalSecrets = Mathf.Max(data.totalSecrets, result.SecretsFound);

        if (data.bestCompletionTime <= 0f || result.CompletionTime < data.bestCompletionTime)
            data.bestCompletionTime = result.CompletionTime;

        if (data.bestDeaths < 0 || result.Deaths < data.bestDeaths)
            data.bestDeaths = result.Deaths;

        if (RankValue(result.Rank) > RankValue(data.bestRank))
            data.bestRank = result.Rank;

        ProgressionSave.Save(data);
    }

    private static int RankValue(string rank)
    {
        switch (rank)
        {
            case "S": return 4;
            case "A": return 3;
            case "B": return 2;
            case "C": return 1;
            default: return 0;
        }
    }
}
