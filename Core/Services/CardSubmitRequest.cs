using System.ComponentModel.DataAnnotations;

namespace Core.Services;

public class CardSubmitRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 2)]
    public string Answer { get; set; }

    public int UserProviderId { get; set; }
}