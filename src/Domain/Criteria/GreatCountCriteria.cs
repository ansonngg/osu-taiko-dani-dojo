using OsuTaikoDaniDojo.Domain.Entity;

namespace OsuTaikoDaniDojo.Domain.Criteria;

public class GreatCountCriteria(int threshold) : CriteriaBase(threshold)
{
    public override bool IsSatisfied(StageResult stageResult)
    {
        return stageResult.GreatCount >= Threshold;
    }
}
