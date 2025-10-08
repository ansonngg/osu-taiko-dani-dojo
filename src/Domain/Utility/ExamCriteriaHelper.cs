using OsuTaikoDaniDojo.Domain.Entity;

namespace OsuTaikoDaniDojo.Domain.Utility;

public static class ExamCriteriaHelper
{
    public static void Initialize(this ExamCriteria[] criteriaList)
    {
        if (criteriaList.Length != (int)PassType.Count)
        {
            throw new ArgumentException("Invalid criteria list length.");
        }

        for (var i = 0; i < criteriaList.Length; i++)
        {
            criteriaList[i] = new ExamCriteria();
        }
    }

    public static PassType Check(
        this ExamCriteria[] criteriaList,
        int greatCount,
        int okCount,
        int missCount,
        int largeBonusCount,
        int maxCombo,
        int hitCount,
        PassType highestPassType)
    {
        for (var i = (int)highestPassType; i >= 0; i--)
        {
            if (criteriaList[i].Check(greatCount, okCount, missCount, largeBonusCount, maxCombo, hitCount))
            {
                return (PassType)i;
            }
        }

        return PassType.Invalid;
    }
}
