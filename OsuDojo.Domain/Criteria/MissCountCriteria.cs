using OsuDojo.Domain.Entity;

namespace OsuDojo.Domain.Criteria;

public class MissCountCriteria(int threshold) : CriteriaBase(threshold)
{
    public override bool IsSatisfied(StageResult stageResult)
    {
        return stageResult.MissCount < Threshold;
    }
}
