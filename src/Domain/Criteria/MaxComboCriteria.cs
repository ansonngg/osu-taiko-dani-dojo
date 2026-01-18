using OsuTaikoDaniDojo.Domain.Entity;

namespace OsuTaikoDaniDojo.Domain.Criteria;

public class MaxComboCriteria(int threshold) : CriteriaBase(threshold)
{
    public override bool IsSatisfied(StageResult stageResult)
    {
        return stageResult.MaxCombo >= Threshold;
    }
}