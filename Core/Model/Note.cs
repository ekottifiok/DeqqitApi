using Core.Model.Interface;

namespace Core.Model;

public class Note : BaseModel, IPagination<int>
{
    public int DeckId { get; set; }
    public Deck Deck { get; set; }
    public int NoteTypeId { get; set; }
    public NoteType NoteType { get; set; }
    public required string CreatorId { get; set; }
    public User Creator { get; set; }
    public Dictionary<string, string> Data { get; set; }
    public List<string> Tags { get; set; }
    public ICollection<Card> Cards { get; set; } = [];
    public int Id { get; set; }
}