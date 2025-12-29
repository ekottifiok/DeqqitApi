using Core.Model.Interface;
using Microsoft.EntityFrameworkCore;

namespace Core.Model;

[Index(nameof(Name), nameof(Id), IsUnique = true)]
public class Deck : BaseModel, IPagination<int>
{
    public string CreatorId { get; set; }
    public User Creator { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DeckOption? Option { get; set; }
    public ICollection<Note> Notes { get; set; } = [];
    public ICollection<Card> Cards { get; set; } = [];
    public ICollection<DeckDailyCount> DailyCounts { get; set; } = [];
    public int Id { get; set; }
}