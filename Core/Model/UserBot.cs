using Core.Model.Helper;
using Microsoft.EntityFrameworkCore;

namespace Core.Model;

[Index(nameof(BotId), nameof(UserId), IsUnique = true)]
public class UserBot
{
    public int Id { get; set; }
    public required string BotId { get; set; }
    public required string UserId { get; set; }
    public User User { get; set; }
    public UserBotType Type { get; set; }
}