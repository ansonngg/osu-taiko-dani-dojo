using OsuDojo.Domain.Entity;

namespace OsuDojo.Domain.Criteria;

public class GreatCountCriteria(int threshold) : CriteriaBase(threshold)
{
    public override bool IsSatisfied(StageResult stageResult)
    {
        return stageResult.GreatCount >= Threshold;
    }
}
