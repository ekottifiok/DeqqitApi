using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Core.Model;

public class User : IdentityUser
{
    [Required]
    public string ProfileImageUrl { get; set; } = "https://avatar.iran.liara.run/public";
    public List<UserRefreshToken> RefreshTokens { get; set; } = [];
    public List<DateOnly> UserStreaks { get; set; } = [];
    public DeckOption DeckOption { get; set; } = DeckOption.CreateDefault;
    public ICollection<Deck> Decks { get; set; } = [];
    public ICollection<NoteType> NoteTypes { get; set; } = [];
    public List<UserAiProvider> AiProviders { get; set; } = [];
    public ICollection<UserBot> UserBots { get; set; } = [];
}