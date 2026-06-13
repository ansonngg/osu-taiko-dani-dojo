using OsuDojo.Application.Query;

namespace OsuDojo.Application.Interface;

public interface IUserRepository
{
    Task<UserRoleQuery?> GetUserRoleAsync(int osuId);
    Task<UserRoleQuery> CreateAsync(int osuId);
    Task UpdateHighestGradeCertificateAsync(int userId, int gradeCertificateId);
}
