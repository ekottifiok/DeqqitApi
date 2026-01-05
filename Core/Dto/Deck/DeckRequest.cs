using System.ComponentModel.DataAnnotations;

namespace Core.Dto.Deck;

public class DeckRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string Name { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public required string Description { get; set; }

    public DeckOptionRequest? OptionRequest { get; set; }
}