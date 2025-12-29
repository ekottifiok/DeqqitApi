using Core.Model.Interface;

namespace Core.Model;

public class NoteType : BaseModel, IPagination<int>
{
    public string? CreatorId { get; set; }
    public User? Creator { get; set; }
    public string Name { get; set; }
    public string CssStyle { get; set; }
    public ICollection<NoteTypeTemplate> Templates { get; set; } = [];
    public int Id { get; set; }
}