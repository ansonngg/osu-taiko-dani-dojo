namespace Application.Model;

public class UserSession
{
    public int UserId { get; init; }
    public UserToken UserToken { get; init; } = new();
}
