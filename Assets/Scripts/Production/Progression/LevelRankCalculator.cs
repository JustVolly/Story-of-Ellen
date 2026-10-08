using UnityEngine;

public static class LevelRankCalculator
{
    public static string Calculate(float completionTime, float sTime, float aTime, int deaths, int memoriesFound, int memoriesTotal)
    {
        bool allMemories = memoriesTotal <= 0 || memoriesFound >= memoriesTotal;
        if (completionTime <= sTime && deaths == 0 && allMemories) return "S";
        if (completionTime <= aTime && deaths <= 1) return "A";
        if (deaths <= 3) return "B";
        return "C";
    }
}
