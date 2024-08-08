using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgBOT
{
    class Program
    {
        private static ITelegramBotClient botClient;

        static async Task Main(string[] args)
        {
            botClient = new TelegramBotClient("7408198729:AAGcqCxOGDnXBqxqeaaOzBtHq3Bx_eS73KI");

            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Hello, I am user {me.Id} and my name is {me.FirstName}.");

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );

            Console.WriteLine("Bot is up and running. Press any key to exit.");
            Console.ReadKey();

            cts.Cancel();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;

                string response = messageText switch
                {
                    "/start" => "Welcome! I am your friendly bot.",
                    "/help" => "Available commands:\n/start - Welcome message\n/help - List of commands\n/echo <message> - Repeat your message\n/time - Current time",
                    _ => HandleCustomCommands(messageText)
                };

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: response,
                    cancellationToken: cancellationToken
                );
            }
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private static string HandleCustomCommands(string message)
        {
            if (message.StartsWith("/echo"))
            {
                var echoMessage = message.Substring(6);
                return string.IsNullOrWhiteSpace(echoMessage) ? "Usage: /echo <message>" : echoMessage;
            }
            else if (message == "/time")
            {
                return $"Current time: {DateTime.Now.ToLongTimeString()}";
            }
            else
            {
                return "Unknown command. Type /help to see available commands.";
            }
        }
    }
}