using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace UKLepraBotConsole
{

    public static class Program
    {
        private static TelegramBotClient _bot;
        private static ChatSettings _chatSettings;
        private static ReactionsList _reactions;

        public static async Task Main()
        {
            _bot = new TelegramBotClient(Configuration.BotToken);

            var me = await _bot.GetMeAsync();
            Console.Title = me.Username;

            var info = _bot.GetWebhookInfoAsync();
            var cts = new CancellationTokenSource();

            var a = await _bot.MakeRequestAsync<Boolean>((IRequest<Boolean>)new DeleteWebhookRequest(), cts.Token);

            Configuration.StartupTime = DateTimeOffset.UtcNow;

            LoadChatSettings();
            LoadReactions();


            

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            _bot.StartReceiving(
                new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                cts.Token
            );

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }

        public static async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(Message message)
        {
            if(message.Type == MessageType.ChatMembersAdded && message.NewChatMembers.Any())
            {
                var newUser = message.NewChatMembers.First();//(x => x.IsBot == false);

                var name = $"{newUser.FirstName} {newUser.LastName}".TrimEnd();
                var reply = $"[{name}](tg://user?id={newUser.Id}), ты вообще с какого посткода";

                await _bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        text: reply,
                        parseMode:ParseMode.MarkdownV2);
                
                Console.WriteLine("Processed ChatMembersAdded event");
                return;
            }

            if(message.Type == MessageType.ChatMemberLeft)
            {
                var sticker = new InputOnlineFile(Stickers.DaIHuiSNim);
                await _bot.SendStickerAsync(chatId: message.Chat.Id, sticker: sticker);

                Console.WriteLine("Processed ChatMemberLeft event");
                return;
            }

            if(message.Type == MessageType.Text)
            {
                var messageAdapterFactory = new MessageAdapterFactory(_bot, _chatSettings,_reactions);

                var messageAdapter = messageAdapterFactory.CreateAdapter(message);
                await messageAdapter.Process(message);
            }
        }

        private static async Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
        }

        public static async Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
        }


        private static void LoadChatSettings()
        {
            //var settingsString = AzureStorageAdapter.ReadBlobFromSettings("chatsettings.json");
            //_chatSettings = JsonConvert.DeserializeObject<ChatSettings>(settingsString);

            //using(var reader = new StreamReader("../../../chatsettings.json"))            
            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UKLepraBotConsole.chatsettings.json"))
            using(var reader = new StreamReader(stream))
            {
                var chatSettingsString = reader.ReadToEnd();
                _chatSettings = JsonConvert.DeserializeObject<ChatSettings>(chatSettingsString);
            }

            if (_chatSettings == null)
                _chatSettings = new ChatSettings();
        }

        private static void LoadReactions()
        {
            //using (var reader = new StreamReader("../../../reactions.json"))
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UKLepraBotConsole.reactions.json"))
            using (var reader = new StreamReader(stream))
            {
                var reactionsString = reader.ReadToEnd();
                _reactions = JsonConvert.DeserializeObject<ReactionsList>(reactionsString);
            }
        }
    }
}
