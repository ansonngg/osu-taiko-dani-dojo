using OsuDojo.Domain.Entity;

namespace OsuDojo.Domain.Criteria;

public abstract class CriteriaBase(int threshold)
{
    protected int Threshold { get; } = threshold;

    public abstract bool IsSatisfied(StageResult stageResult);
}
