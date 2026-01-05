using Core.Dto.Common;
using Core.Model.Helper;
using Core.Services.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Helpers;

namespace TelegramBot.Services;

public class AuthHandler(IUserBotCodeService userBotCodeService)
{
    public async Task HandleAuth(ITelegramBotClient bot, Message message, CancellationToken ct)
    {
        if (message.From is not { } sender) return;
        string text = message.Text ?? string.Empty;

        string token = text[6..].Trim();
        if (string.IsNullOrEmpty(token))
        {
            await MessageHelper.SendError(bot, message.Chat.Id, "Please provide a token. Usage: /auth <token>", ct);
            return;
        }

        ResponseResult<bool> result = await userBotCodeService.VerifyCode(token, sender.Id.ToString(), UserBotType.Telegram);
        if (!result.IsSuccess || !result.Value)
        {
            await MessageHelper.SendError(bot, message.Chat.Id, "Invalid Code. Please retry", ct);
            return;
        }

        await bot.SendMessage(
            message.Chat.Id,
            "✅ Authenticated successfully! You can now use the menu.",
            replyMarkup: KeyboardHelper.MainMenu,
            cancellationToken: ct);
    }
}