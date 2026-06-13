using OsuDojo.Domain.Entity;

namespace OsuDojo.Domain.Criteria;

public class HitCountCriteria(int threshold) : CriteriaBase(threshold)
{
    public override bool IsSatisfied(StageResult stageResult)
    {
        return stageResult.HitCount >= Threshold;
    }
}
