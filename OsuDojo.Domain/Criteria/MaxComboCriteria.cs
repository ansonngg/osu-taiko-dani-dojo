using OsuDojo.Domain.Entity;

namespace OsuDojo.Domain.Criteria;

public class MaxComboCriteria(int threshold) : CriteriaBase(threshold)
{
    public override bool IsSatisfied(StageResult stageResult)
    {
        return stageResult.MaxCombo >= Threshold;
    }
}
