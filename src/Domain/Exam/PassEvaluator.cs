using OsuTaikoDaniDojo.Domain.Criteria;
using OsuTaikoDaniDojo.Domain.Entity;
using OsuTaikoDaniDojo.Domain.Utility;

namespace OsuTaikoDaniDojo.Domain.Exam;

public class PassEvaluator
{
    private readonly List<CriteriaSet> _criteriaSets = [];

    public void SetUpCriteria(
        int[]? greatCounts,
        int[]? okCounts,
        int[]? missCounts,
        int[]? largeBonusCounts,
        int[]? maxCombos,
        int[]? hitCounts)
    {
        _LoadCriteria(greatCounts, x => new GreatCountCriteria(x));

        // TODO: Set up all other types of criteria as well
    }

    public int Evaluate(StageResult stageResult)
    {
        for (var i = _criteriaSets.Count - 1; i >= 0; i--)
        {
            if (_criteriaSets[i].IsPassed(stageResult))
            {
                return i + 1;
            }
        }

        return 0;
    }

    private void _LoadCriteria(int[]? thresholds, Func<int, CriteriaBase> constructCriteria)
    {
        if (thresholds == null)
        {
            return;
        }

        _criteriaSets.Extend(thresholds.Length);

        for (var i = 1; i <= thresholds.Length; i++)
        {
            _criteriaSets[^i].Add(constructCriteria(thresholds[^i]));
        }
    }
}
