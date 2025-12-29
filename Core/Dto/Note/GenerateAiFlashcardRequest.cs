using System.ComponentModel.DataAnnotations;

namespace Core.Dto.Note;

public class GenerateAiFlashcardRequest
{
    [Required] public int ProviderId { get; set; }
    [Required] public int NoteTypeId { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 3)]
    public string Description { get; set; }
}