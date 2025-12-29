using Core.Model.Helper;
using Core.Model.Interface;

namespace Core.Model;

public class DeckDailyCount : BaseModel, IPagination<int>
{
    public int DeckId { get; set; }
    public Deck Deck { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public CardState CardState { get; set; }
    public int Count { get; set; } = 0;
    public int Id { get; set; }
}