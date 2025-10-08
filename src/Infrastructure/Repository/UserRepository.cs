using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Infrastructure.Model;
using Supabase.Postgrest;
using Client = Supabase.Client;

namespace OsuTaikoDaniDojo.Infrastructure.Repository;

public class UserRepository(Client database) : IUserRepository
{
    private readonly Client _database = database;

    public async Task<string?> GetUserRoleAsync(int osuId)
    {
        var response = await _database
            .From<User>()
            .Select(x => new object[] { x.Role })
            .Where(x => x.OsuId == osuId)
            .Single();

        return response?.Role;
    }

    public async Task<string> CreateAsync(int osuId)
    {
        var response = await _database
            .From<User>()
            .Insert(
                new User { OsuId = osuId },
                new QueryOptions { Returning = QueryOptions.ReturnType.Representation });

        return response.Model?.Role ?? throw new NullReferenceException("Returned user is null.");
    }
}
