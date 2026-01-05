using System.ComponentModel.DataAnnotations;

namespace Core.Dto.Card;

public class CardSubmitRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 2)]
    public required string Answer { get; set; }

    public int UserProviderId { get; set; }
}