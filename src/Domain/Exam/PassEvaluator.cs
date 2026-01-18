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
        _LoadCriteria(okCounts, x => new OkCountCriteria(x));
        _LoadCriteria(missCounts, x => new MissCountCriteria(x));
        _LoadCriteria(largeBonusCounts, x => new LargeBonusCountCriteria(x));
        _LoadCriteria(maxCombos, x => new MaxComboCriteria(x));
        _LoadCriteria(hitCounts, x => new HitCountCriteria(x));
    }

    public int Evaluate(StageResult stageResult)
    {
        for (var i = 0; i < _criteriaSets.Count; i++)
        {
            if (_criteriaSets[i].IsPassed(stageResult))
            {
                return _criteriaSets.Count - i;
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

        for (var i = 0; i < thresholds.Length; i++)
        {
            _criteriaSets[i].Add(constructCriteria(thresholds[^(i + 1)]));
        }
    }
}
