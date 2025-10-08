namespace OsuTaikoDaniDojo.Application.Interface;

public interface IUserRepository
{
    Task<string?> GetUserRoleAsync(int osuId);
    Task<string> CreateAsync(int osuId);
}
