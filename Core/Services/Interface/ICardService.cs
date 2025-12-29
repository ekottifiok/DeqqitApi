using Core.Dto.Card;
using Core.Dto.Common;
using Core.Model;

namespace Core.Services.Interface;

public interface ICardService
{
    Task<PaginationResult<Card>> Get(string creatorId, int deckId, PaginationRequest<int> request);

    Task<int> UpdateCardState(string creatorId, int id, UpdateCardStateRequest request);

    Task<CardResponse?> GetNextStudyCard(string creatorId, int deckId);

    Task<bool> SubmitCardReview(string creatorId, int id, CardSubmitRequest request);
}