using System.ComponentModel.DataAnnotations;
using Core.Dto.Validation;

namespace Core.Dto.Note;

public class NoteRequest
{
    [Required] public required Dictionary<string, string> Data { get; set; }

    [AlphabetsOnly] [MaxLength(20)] public List<string> Tags { get; set; } = [];
}