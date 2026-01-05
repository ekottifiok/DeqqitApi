using System.ComponentModel.DataAnnotations;

namespace Core.Dto.NoteType;

public class NoteTypeRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string Name { get; set; }

    [StringLength(5000)] public required string CssStyle { get; set; } = string.Empty;
    [Required] [MinLength(1)] public List<UpsertNoteTypeTemplateRequest> Templates { get; set; }
}