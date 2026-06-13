using OsuDojo.Domain.Entity;

namespace OsuDojo.Domain.Criteria;

public class LargeBonusCountCriteria(int threshold) : CriteriaBase(threshold)
{
    public override bool IsSatisfied(StageResult stageResult)
    {
        return stageResult.LargeBonusCount >= Threshold;
    }
}
