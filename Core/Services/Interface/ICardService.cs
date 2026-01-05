using Core.Dto.Card;
using Core.Dto.Common;
using Core.Model;

namespace Core.Services.Interface;

public interface ICardService
{
    Task<PaginationResult<Card>> Get(string creatorId, int deckId, PaginationRequest<int> request);
    Task<ResponseResult<Card>> Get(string creatorId, int id);
    Task<ResponseResult<bool>> UpdateCardState(string creatorId, int id, UpdateCardStateRequest request);
    Task<ResponseResult<CardResponse>> GetNextStudyCard(string creatorId, int deckId);
    Task<ResponseResult<CardResponse>> SubmitCardReview(string creatorId, int id, CardSubmitRequest request);
}