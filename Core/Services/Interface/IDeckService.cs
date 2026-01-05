using Core.Dto.Common;
using Core.Dto.Deck;
using Core.Model;

namespace Core.Services.Interface;


public interface IDeckService
{
    Task<ResponseResult<Deck>> Create(string creatorId, CreateDeckRequest request);
    Task<ResponseResult<bool>> Update(int id, string creatorId, UpdateDeckRequest request);
    Task<ResponseResult<bool>> Delete(int id, string creatorId);
    Task<ResponseResult<DeckStatisticsResponse>> GetStatistics(string creatorId, int id);
    Task<ResponseResult<DeckSummaryResponse>> GetSummary(string creatorId, int id);
}