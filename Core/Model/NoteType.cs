using Core.Model.Interface;
using Microsoft.EntityFrameworkCore;

namespace Core.Model;

[Index(nameof(Name), nameof(CreatorId), IsUnique = true)]
public class NoteType : BaseModel, IPagination<int>
{
    public string? CreatorId { get; set; }
    public User? Creator { get; set; }
    public required string Name { get; set; }
    public required string CssStyle { get; set; }
    public ICollection<NoteTypeTemplate> Templates { get; set; } = [];
    public int Id { get; set; }
}