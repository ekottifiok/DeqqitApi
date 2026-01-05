using Core.Model.Interface;

namespace Core.Model;

public class NoteTypeTemplate : BaseModel, IPagination<int>
{
    public int NoteTypeId { get; set; }
    public NoteType NoteType { get; set; }
    public required string Front { get; set; }
    public required string Back { get; set; }
    public int Id { get; set; }
}