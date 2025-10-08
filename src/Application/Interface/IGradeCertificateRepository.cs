namespace OsuTaikoDaniDojo.Application.Interface;

public interface IGradeCertificateRepository
{
    Task<int> CreateAsync(
        int userId,
        int grade,
        int passLevel,
        int[] greatCounts,
        int[] okCounts,
        int[] missCounts,
        int[] largeBonusCounts,
        int[] maxCombos,
        int[] hitCounts,
        int examSessionId);
}
