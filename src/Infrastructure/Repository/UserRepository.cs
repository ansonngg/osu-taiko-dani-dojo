using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Infrastructure.Model;
using Supabase.Postgrest;
using Client = Supabase.Client;

namespace OsuTaikoDaniDojo.Infrastructure.Repository;

public class UserRepository(Client database) : IUserRepository
{
    private readonly Client _database = database;

    public async Task<UserRoleQuery?> GetUserRoleAsync(int osuId)
    {
        var response = await _database
            .From<User>()
            .Select(x => new object[] { x.Id, x.Role })
            .Where(x => x.OsuId == osuId)
            .Single();

        if (response == null)
        {
            return null;
        }

        var role = $"{char.ToUpperInvariant(response.Role[0])}{response.Role[1..]}";
        return new UserRoleQuery { UserId = response.Id, Role = role };
    }

    public async Task<UserRoleQuery> CreateAsync(int osuId)
    {
        var response = await _database
            .From<User>()
            .Insert(
                new User { OsuId = osuId },
                new QueryOptions { Returning = QueryOptions.ReturnType.Representation });

        return response.Model != null
            ? new UserRoleQuery { UserId = response.Model.Id, Role = response.Model.Role }
            : throw new NullReferenceException("Returned user is null.");
    }

    public async Task UpdateHighestGradeCertificateAsync(int userId, int gradeCertificateId)
    {
        await _database.Rpc(
            "update_user_highest_grade_certificate",
            new { p_user_id = userId, p_grade_certificate_id = gradeCertificateId });
    }
}
