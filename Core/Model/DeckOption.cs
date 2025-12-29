using Core.Model.Helper;
using Microsoft.EntityFrameworkCore;

namespace Core.Model;

[Owned]
public record DeckOption
{
    public int NewLimitPerDay { get; set; }
    public int ReviewLimitPerDay { get; set; }
    public DeckOptionSortOrder SortOrder { get; set; }
    public bool InterdayLearningMix { get; set; }

    public static DeckOption CreateDefault => new()
    {
        NewLimitPerDay = 20,
        ReviewLimitPerDay = 200,
        SortOrder = DeckOptionSortOrder.DueDate,
        InterdayLearningMix = false
    };
}