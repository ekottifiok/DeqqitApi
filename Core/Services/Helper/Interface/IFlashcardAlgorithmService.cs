using Core.Dto.Common;
using Core.Model.Helper;

namespace Core.Services.Helper.Interface;

public interface IFlashcardAlgorithmService
{
    double EstimateRetention(double ef);
    FlashcardResult Calculate(int quality, CardState currentState, int interval, double easeFactor, int stepIndex);
}