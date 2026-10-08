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
        ProgressionSave.RecordLevelResult(result);
    }
}
