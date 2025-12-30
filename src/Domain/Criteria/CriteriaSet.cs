using OsuTaikoDaniDojo.Domain.Entity;

namespace OsuTaikoDaniDojo.Domain.Criteria;

public class CriteriaSet
{
    private readonly Dictionary<Type, CriteriaBase> _criteriaMap = new();

    public void Add(CriteriaBase criteria)
    {
        _criteriaMap.Add(criteria.GetType(), criteria);
    }

    public bool IsPassed(StageResult stageResult)
    {
        return _criteriaMap.Values.All(x => x.IsSatisfied(stageResult));
    }
}
