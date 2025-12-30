using OsuTaikoDaniDojo.Domain.Entity;

namespace OsuTaikoDaniDojo.Domain.Criteria;

public abstract class CriteriaBase(int threshold)
{
    protected int Threshold { get; } = threshold;

    public abstract bool IsSatisfied(StageResult stageResult);
}
