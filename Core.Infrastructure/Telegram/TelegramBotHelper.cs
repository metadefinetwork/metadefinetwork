using Core.Application.ViewModels.BotTelegram;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Infrastructure.Telegram

{
    public class TelegramBotHelper
    {
        public static string BuildReportWITHDRAWMessage(WithdrawMessageParam model)
        {
            return $"<b>{model.Title.ToUpper()}</b>\n{model.Email} WITHDRAW {model.Amount} {model.Currency}\n" +
                   //$"<b>User ID: MAR{model.UserId}</b>\n" +
                   //$"EMAIL: <b>{model.Email}</b>\n" +
                   $"Address: <b>{model.Wallet}</b>\n" +
                   (model.Rate > 0 ? $"RATE: <b>${model.Rate}</b>\n" : "") +
                   $"{model.Currency} AMOUNT: {model.Amount} {model.Currency}\n" +
                   $"Fee AMOUNT: <b>{model.Fee} {model.Currency}</b>\n" +
                   //$"<b>Sponsor ID: MAR{model.SponsorId}</b>\n" +
                   $"<b>Sponsor Id: {model.SponsorEmail}</b>\n" +
                   $"<b>Submit Withdraw Time:</b> {model.WithDrawTime.ToString("dd-MM-yyyy HH:mm:ss")}\n";
        }

        public static string BuildReportDEPOSITMessage(DepositMessageParam model)
        {
            return $"{model.Email} DEPOSIT {model.Amount} {model.Currency}\n" +
                   //$"<b>User ID: {model}{model.UserId}</b>\n" +
                   $"<b>Amount {model.Currency}:</b> {model.Amount} {model.Currency}\n" +
                   $"Address: {model.Wallet}\n" +
                   (model.Rate > 0 ? $"RATE: <b>${model.Rate}</b>\n" : "") +
                   //$"<b>Sponsor ID: MAR{model.SponsorId}</b>\n" +
                   $"<b>Sponsor Id: {model.SponsorEmail}</b>\n" +
                   $"<b>Deposit Time:</b> {model.DepositeTime.ToString("dd-MM-yyyy HH:mm:ss")}\n";
        }

        public static string BuildReportDepositPresaleMessage(DeFiMessageParam model)
        {
            return $"<b>{model.Title.ToUpper()}</b>\nUser DEPOSIT {model.AmountBNB} BNB to purchase {model.AmountToken} MAR\n" +
                   $"<b>Amount BNB:</b> {model.AmountBNB} BNB\n" +
                   $"<b>Amount {model.Currency}:</b> {model.AmountToken} MAR\n" +
                   $"<b>User Wallet:</b> {model.UserWallet}\n" +
                   $"<b>User Email:</b> {model.Email}\n" +
                   $"<b>Receiving System Wallet:</b> {model.SystemWallet}\n" +
                   $"<b>Deposit Time:</b> {model.DepositAt.ToString("dd-MM-yyyy HH:mm:ss")}\n";
        }

        public static string BuildReportReceivePresaleMessage(DeFiMessageParam model)
        {
            return $"<b>{model.Title.ToUpper()}</b>\nUser received {model.AmountToken} MAR\n" +
                   $"<b>Amount BNB:</b> {model.AmountBNB} BNB\n" +
                   $"<b>Amount {model.Currency}:</b> {model.AmountToken} MAR\n" +
                   $"<b>User Wallet:</b> {model.UserWallet}\n" +
                   $"<b>User Email:</b> {model.Email}\n" +
                   $"<b>Sending System Wallet:</b> {model.SystemWallet}\n" +
                   $"<b>Receive Time:</b> {model.ReceivedAt.ToString("dd-MM-yyyy HH:mm:ss")}\n";
        }

        public static List<TelegramBotQueueModel> Bots => new List<TelegramBotQueueModel>
        {
            new TelegramBotQueueModel
            {
                BotClient = new TelegramBotClient(BotToken1),
            },
            new TelegramBotQueueModel
            {
                BotClient = new TelegramBotClient(BotToken2),
            },
            new TelegramBotQueueModel
            {
                BotClient = new TelegramBotClient(BotToken3),
            },
            new TelegramBotQueueModel
            {
                BotClient = new TelegramBotClient(BotToken4),
            },
            new TelegramBotQueueModel
            {
                BotClient = new TelegramBotClient(BotToken5),
            },
            new TelegramBotQueueModel
            {
                BotClient = new TelegramBotClient(BotToken6),
            },
            new TelegramBotQueueModel
            {
                BotClient = new TelegramBotClient(BotToken7),
            },
            new TelegramBotQueueModel
            {
                BotClient = new TelegramBotClient(BotToken8),
            },
        };

        //Group Chat
        public static ChatId WithdrawGroup => new ChatId(-672626743);

        public static ChatId DepositGroup => new ChatId(-773846752);
        public static ChatId TestGroup => new ChatId(-573514189);

        //Bot Telegram
        public const string BotToken1 = "1953768449:AAEYkd6ZrQPG1Wv-4jt-wpS1URLdVxt8wuU";

        public const string BotToken2 = "1989172401:AAFuWnReLMXUDOjfTYX-dIUrNcxyHUs7IOA";
        public const string BotToken3 = "1961607617:AAGdPP4tYxd5VokvY-Ch2lC3ybY7EO8YfHQ";
        public const string BotToken4 = "1970681615:AAEaDNWxIicqiIH5FezPhtMzBhJxh7ytoT8";
        public const string BotToken5 = "1995360693:AAG_wKpmWiN8pXX9Z6NfkwgljjKliZegG3w";

        public const string BotToken6 = "1984098820:AAEDWW_rkFIDZ-lp8ysNgo5CTpYrrCbma-Q";
        public const string BotToken7 = "1926615802:AAGS-Z7cLgYo_oSx38k0vfM2rw-PB9KULg8";
        public const string BotToken8 = "1978865032:AAGhbrq-x98SFXR8eREkoKe2hKfK9tKFjP4";
    }
}
