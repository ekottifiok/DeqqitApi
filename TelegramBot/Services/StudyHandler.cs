using Core.Dto.Card;
using Core.Services.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Services;

public class StudyHandler(ICardService cardService, IUserService userService, IUserBotService userBotService)
{
    public async Task StartStudy(ITelegramBotClient bot, CallbackQuery query, string telegramUserId, int deckId,
        CancellationToken ct)
    {
        var userResult = await userBotService.Get(telegramUserId);
        if (!userResult.IsSuccess || userResult.Value == null) return;
        string userId = userResult.Value;

        var result = await cardService.GetNextStudyCard(userId, deckId);
        if (!result.IsSuccess || result.Value is null)
        {
            await bot.EditMessageText(
                query.Message!.Chat.Id,
                query.Message.MessageId,
                "🎉 *All caught up!* No more cards to study for now.",
                ParseMode.Markdown,
                cancellationToken: ct);
            return;
        }

        await SendCardFront(bot, query.Message!.Chat.Id, query.Message.MessageId, result.Value, deckId, ct);
    }

    private async Task SendCardFront(ITelegramBotClient bot, long chatId, int messageId, CardResponse card, int deckId,
        CancellationToken ct)
    {
        // Hidden metadata: [CardId:DeckId]
        string hiddenMeta = $"[{card.Id}:{deckId}]";
        // We use a zero-width space or just append it at the end in a way we can parse.
        // Actually, let's just put it in the text.
        string text =
            $"📝 *Front*\n\n{card.Front}\n\n_Reply to this message with your answer._\n\n`ID:{card.Id}:{deckId}`";

        await bot.DeleteMessage(chatId, messageId, ct);

        await bot.SendMessage(
            chatId,
            text,
            ParseMode.Markdown,
            replyMarkup: new ForceReplyMarkup { InputFieldPlaceholder = "Type your answer..." },
            cancellationToken: ct);
    }

    public async Task SubmitAnswer(ITelegramBotClient bot, Message message, string answer, int cardId, int deckId,
        CancellationToken ct)
    {
        var userResult = await userBotService.Get(message.From!.Id.ToString());
        if (!userResult.IsSuccess || userResult.Value == null) return;
        string userId = userResult.Value;

        var providerResult = await userService.GetDefaultProviderId(userId);
        if (!providerResult.IsSuccess || providerResult.Value == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ No AI Provider found. Please configure one in the app.",
                cancellationToken: ct);
            return;
        }

        CardSubmitRequest request = new()
        {
            Answer = answer,
            UserProviderId = providerResult.Value.Value
        };

        var result = await cardService.SubmitCardReview(userId, cardId, request);

        if (result.IsSuccess && result.Value != null)
        {
            var response = result.Value;
            string feedback =
                $"✅ *Review Submitted*\n\n📝 *Front*: {response.Front}\n📖 *Back*: {response.Back}\n\n_Next card loading..._";
            await bot.SendMessage(message.Chat.Id, feedback, ParseMode.Markdown, cancellationToken: ct);

            // Start next card
            var nextResult = await cardService.GetNextStudyCard(userId, deckId);
            if (nextResult.IsSuccess && nextResult.Value != null)
                // We pass a dummy message ID since we are sending a new message anyway
                await SendCardFront(bot, message.Chat.Id, message.MessageId, nextResult.Value, deckId, ct);
            else
                await bot.SendMessage(message.Chat.Id, "🎉 *All caught up!*", ParseMode.Markdown,
                    cancellationToken: ct);
        }
        else
        {
            await bot.SendMessage(message.Chat.Id, "❌ Error submitting review. Please try again.",
                cancellationToken: ct);
        }
    }
}