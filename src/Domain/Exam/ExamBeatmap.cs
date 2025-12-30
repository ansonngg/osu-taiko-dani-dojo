using OsuTaikoDaniDojo.Domain.Entity;

namespace OsuTaikoDaniDojo.Domain.Exam;

public class ExamBeatmap(int beatmapId, int playlistId, int length)
{
    private readonly PassEvaluator _passEvaluator = new();

    public int Id { get; } = beatmapId;
    public int PlaylistId { get; } = playlistId;
    public int Length { get; } = length;
    public StageResult Result { get; private set; } = new();

    public void SetUpCriteria(
        int[]? greatCounts,
        int[]? okCounts,
        int[]? missCounts,
        int[]? largeBonusCounts,
        int[]? maxCombos,
        int[]? hitCounts)
    {
        _passEvaluator.SetUpCriteria(greatCounts, okCounts, missCounts, largeBonusCounts, maxCombos, hitCounts);
    }

    public int Evaluate(StageResult stageResult)
    {
        Result = stageResult;
        return _passEvaluator.Evaluate(Result);
    }
}
