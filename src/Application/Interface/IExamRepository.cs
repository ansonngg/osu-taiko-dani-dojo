using OsuTaikoDaniDojo.Application.Query;

namespace OsuTaikoDaniDojo.Application.Interface;

public interface IExamRepository
{
    public Task<ExamQuery[]> GetAllAsync();
    public Task<ExamQuery?> GetByGradeAsync(int grade);
    public Task CreateAsync(int grade, ExamQuery examQuery);
}
