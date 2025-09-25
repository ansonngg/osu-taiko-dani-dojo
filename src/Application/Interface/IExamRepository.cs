using OsuTaikoDaniDojo.Application.Query;

namespace OsuTaikoDaniDojo.Application.Interface;

public interface IExamRepository
{
    public Task<ExamQuery?> GetExamByGradeAsync(int grade);
}
