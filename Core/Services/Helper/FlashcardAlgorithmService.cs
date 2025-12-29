using Core.Dto.Common;
using Core.Model.Helper;
using Core.Services.Helper.Interface;

namespace Core.Services.Helper;

public class FlashcardAlgorithmService : IFlashcardAlgorithmService
{
    private const int GraduationInterval = 1; // 1 day

    private const int EasyInterval = 4; // 4 days

    // Typical Anki defaults: 1 min, 10 min
    private static readonly int[] LearningSteps = [1, 10];

    public double EstimateRetention(double ef)
    {
        (double Ef, double Retention)[] table =
        [
            (1.3, 0.575),
            (1.5, 0.65),
            (1.7, 0.72),
            (1.9, 0.78),
            (2.1, 0.83),
            (2.3, 0.87),
            (2.5, 0.90),
            (2.6, 0.93)
        ];

        foreach ((double Ef, double Retention) entry in table)
            if (ef <= entry.Ef)
                return entry.Retention;

        return 0.95; // EF > 2.6
    }


    public FlashcardResult Calculate(int quality, CardState currentState, int interval, double easeFactor,
        int stepIndex)
    {
        FlashcardResult flashcardResult = new()
        {
            Interval = interval, Repetitions = 0, EaseFactor = easeFactor, State = currentState,
            LearningStepIndex = stepIndex
        };
        return currentState switch
        {
            CardState.New => HandleNewOrLearning(quality, easeFactor, 0),
            CardState.Learning => HandleNewOrLearning(quality, easeFactor, stepIndex),
            CardState.Review => HandleReview(quality, interval, easeFactor),
            CardState.Relearning => HandleNewOrLearning(quality, easeFactor, stepIndex),
            CardState.Suspended => flashcardResult,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static FlashcardResult HandleNewOrLearning(int quality, double easeFactor, int stepIndex)
    {
        switch (quality)
        {
            // "Again" - Restart learning steps
            case < 3:
                return new FlashcardResult
                {
                    Interval = LearningSteps[0], EaseFactor = easeFactor, State = CardState.Learning,
                    LearningStepIndex = 0
                };
            // "Easy" - Skip learning and graduate immediately
            case 5:
                return new FlashcardResult
                    { Interval = EasyInterval, EaseFactor = easeFactor, State = CardState.Review, Repetitions = 1 };
        }

        // "Good" - Advance to next step or graduate
        int nextStep = stepIndex + 1;
        return nextStep >= LearningSteps.Length
            ? new FlashcardResult
                { Interval = GraduationInterval, EaseFactor = easeFactor, State = CardState.Review, Repetitions = 1 }
            : new FlashcardResult
            {
                Interval = LearningSteps[nextStep], EaseFactor = easeFactor, State = CardState.Learning,
                LearningStepIndex = nextStep
            };
    }

    private static FlashcardResult HandleReview(int quality, int interval, double easeFactor)
    {
        const int dayToMinute = 1440;
        if (quality < 3) // LAPSE: The user forgot a mature card
        {
            // Anki penalty: Reduce ease factor by 0.20 immediately
            double newEase = Math.Max(1.3, easeFactor - 0.20);
            return new FlashcardResult
            {
                Interval = LearningSteps[0] * dayToMinute, EaseFactor = newEase, State = CardState.Relearning,
                LearningStepIndex = 0
            };
        }

        // Standard SM2 math for "Hard", "Good", "Easy"
        double easeModification = 0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02);
        double nextEaseFactor = Math.Max(1.3, easeFactor + easeModification);

        int nextInterval = quality switch
        {
            3 => (int)Math.Max(interval + 1, interval * 1.2), // Hard: small boost
            4 => (int)Math.Round(interval * easeFactor), // Good: standard boost
            5 => (int)Math.Round(interval * easeFactor * 1.3), // Easy: extra bonus
            _ => interval
        };

        return new FlashcardResult
            { Interval = nextInterval * dayToMinute, EaseFactor = nextEaseFactor, State = CardState.Review };
    }
}