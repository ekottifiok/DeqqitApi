namespace Core.Model;

public class UserRefreshToken
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    public DateTime Validity { get; set; }

    public required string UserId { get; set; }
    public User User { get; set; }
}