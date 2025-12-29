using Core.Dto.Deck;
using Core.Model;

namespace Core.Services.Interface;

public interface IDeckService
{
    Task<DeckSummaryResponse?> GetSummary(string creatorId, int id);
    Task<DeckStatisticsResponse?> GetStatistics(string creatorId, int id);
    Task<Deck?> Create(string creatorId, CreateDeckRequest request);

    Task<int> Update(int id, string creatorId, UpdateDeckRequest request);

    Task<int> Delete(int id, string creatorId);
}