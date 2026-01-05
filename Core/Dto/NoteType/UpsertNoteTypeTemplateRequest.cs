using System.ComponentModel.DataAnnotations;
using Core.Dto.Validation;
using Core.Model;

namespace Core.Dto.NoteType;

public class UpsertNoteTypeTemplateRequest
{
    public int? Id { get; set; }

    [Required]
    [ValidTemplate]
    [StringLength(500, MinimumLength = 2)]
    public required string Front { get; set; }

    [Required]
    [ValidTemplate]
    [StringLength(500, MinimumLength = 2)]
    public required string Back { get; set; }

    // Request -> Entity
    public static implicit operator NoteTypeTemplate(UpsertNoteTypeTemplateRequest request)
    {
        return new NoteTypeTemplate
        {
            // Id = 0,
            Front = request.Front,
            Back = request.Back
        };
    }
}