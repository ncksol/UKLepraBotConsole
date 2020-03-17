using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using UKLepraBotConsole.Adapters;
using UKLepraBotConsole.Models;
using File = System.IO.File;
using Timer = System.Timers.Timer;

namespace UKLepraBotConsole
{
    public static class Program
    {
        private static TelegramBotClient _bot;
        private static ChatSettings _chatSettings;
        private static ReactionsList _reactions;
        private static BoyanList _boyans;
        private static Timer _saveTimer;
        private static bool _isClosing;

        public static async Task Main(string[] args)
        {
            var parser = new Parser(with => with.CaseInsensitiveEnumValues = true);
            var parsedArguments = parser.ParseArguments<CommandLineOptions>(args);

            await parsedArguments.MapResult(async x => { await Run(x); }, async err => { await ShowErrors(err.ToList());});
        }

        private static async Task Run(CommandLineOptions options)
        {            
            Configuration.BotToken = options.BotToken;
            Configuration.TelegramBotId = options.BotId;
            Configuration.SecretKey = options.SecretKey;

#if DEBUG
            if(string.IsNullOrEmpty(Configuration.BotToken))
            {
                Configuration.BotToken = HelperMethods.ReadToken("bot.token");
            }
            if(string.IsNullOrEmpty(Configuration.SecretKey))
            {
                Configuration.SecretKey = HelperMethods.ReadToken("secret.key");
            }
#endif

            _bot = new TelegramBotClient(Configuration.BotToken);

            Console.WriteLine("Choose option:");
            Console.WriteLine("1. Launch bot");
            Console.WriteLine("2. Create webhook");
            Console.WriteLine("3. Get webhook info");
            Console.WriteLine("4. Delete webhook");

            int.TryParse(Console.ReadLine(), out var selectedOption);            

            switch(selectedOption)
            {
                case 1:
                    await RunBot();
                    break;
                case 2:
                    await CreateWebhook();
                    break;
                case 3:
                    await GetWebhookInfo();
                    break;
                case 4:
                    await DeleteWebhook();
                    break;
                default:                    
                    Console.WriteLine("Unknown option");
                    return;
            }
        }

        private static async Task RunBot()
        {
            var me = await _bot.GetMeAsync();
            Console.Title = me.Username;
            Configuration.StartupTime = DateTimeOffset.UtcNow;

            try
            { 
                LoadChatSettings();
                LoadReactions();
                LoadBoyans();

                Console.WriteLine($"Start listening for @{me.Username}");

                var cts = new CancellationTokenSource();

                AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
                {
                    OnExit(cts);
                };

                Console.CancelKeyPress += (s, ev) =>
                {
                    OnExit(cts);
                };

                SetupPeriodicSettingsSaving();

                var updateReceiver = new QueuedUpdateReceiver(_bot);
                updateReceiver.StartReceiving(new UpdateType[] { UpdateType.Message}, HandleErrorAsync, cts.Token);
                await foreach (var update in updateReceiver.YieldUpdatesAsync())
                {
                    await HandleUpdateAsync(update, cts.Token);
                }
            }
            catch(Exception ex)
            {
                _bot.SendTextMessageAsync(chatId: Configuration.MasterId, text: $"Exception:{Environment.NewLine}{ex.Message}").Wait();
            }
        }

        private static void OnExit(CancellationTokenSource cts)
        {
            if(_isClosing) return;

            _isClosing = true;

            _bot.SendTextMessageAsync(chatId: Configuration.MasterId, text: "I am dead").Wait();

            cts.Cancel();
            SaveChatSettings();
            SaveBoyans();
            CleanTemp();
            _saveTimer.Stop();
        }

        private static async Task CreateWebhook()
        {
            Console.WriteLine("Enter webhook url:");
            var url = Console.ReadLine();
            if(string.IsNullOrEmpty(url))
            { 
                Console.WriteLine("Invalid url");
                return;
            }

            var success = await _bot.MakeRequestAsync(new SetWebhookRequest(url, null));
            Console.WriteLine($"Webhook created: {success}");
            Console.ReadKey();
        }
        
        private static async Task GetWebhookInfo()
        {
            var info = await _bot.MakeRequestAsync(new GetWebhookInfoRequest());            
            Console.WriteLine($"Webhook info:");
            Console.WriteLine(JsonConvert.SerializeObject(info));
            Console.ReadKey();
        }
        
        private static async Task DeleteWebhook()
        {
            var success = await _bot.MakeRequestAsync(new DeleteWebhookRequest());
            Console.WriteLine($"Webhook removed: {success}");
            Console.ReadKey();
        }

        private static Task ShowErrors(List<Error> errors)
        {
            foreach (var err in errors)
            {
                if (err is UnknownOptionError unKnownError)
                {
                    Console.WriteLine($"{unKnownError.Token} - {unKnownError.ToString()}");
                }
                else
                {
                    Console.WriteLine(err.ToString());
                }
            }

            return Task.FromResult(0);
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
            if (message.Type == MessageType.ChatMembersAdded && message.NewChatMembers.Any())
            {
                var newUser = message.NewChatMembers.FirstOrDefault(x => x.IsBot == false);
                if (newUser == null) return;

                var name = $"{newUser.FirstName} {newUser.LastName}".TrimEnd();
                var reply = $"[{name}](tg://user?id={newUser.Id}), ты вообще с какого посткода";

                await _bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        text: reply,
                        parseMode: ParseMode.MarkdownV2);

                Console.WriteLine("Processed ChatMembersAdded event");
                return;
            }

            if (message.Type == MessageType.ChatMemberLeft)
            {
                var sticker = new InputOnlineFile(Stickers.DaIHuiSNim);
                await _bot.SendStickerAsync(chatId: message.Chat.Id, sticker: sticker);

                Console.WriteLine("Processed ChatMemberLeft event");
                return;
            }

            if (message.Type == MessageType.Text || message.Type == MessageType.Sticker || (message.Type == MessageType.Document && message.Animation != null) || 
                (message.Type == MessageType.Photo && message.Photo != null))
            {
                var messageAdapterFactory = new MessageAdapterFactory(_bot, _chatSettings, _reactions, _boyans);

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
            var chatSettingsString = ReadSettings("chatsettings.json");
            _chatSettings = JsonConvert.DeserializeObject<ChatSettings>(chatSettingsString);

            if (_chatSettings == null)
                _chatSettings = new ChatSettings();
        }

        private static void LoadReactions()
        {
            var reactionsString = ReadSettings("reactions.json");
            _reactions = JsonConvert.DeserializeObject<ReactionsList>(reactionsString) ?? new ReactionsList();
        }
        
        private static void LoadBoyans()
        {
            var boyansString = ReadSettings("boyans.json");
            _boyans = JsonConvert.DeserializeObject<BoyanList>(boyansString) ?? new BoyanList();
        }

        private static string ReadSettings(string fileName)
        {
            var file = new FileInfo(fileName);
            if (file.Exists == false) return string.Empty;

            var settingString = string.Empty;
            using (var reader = file.OpenText())
            { 
                settingString = reader.ReadToEnd();
            }

            return settingString;
        }

        private static void SaveChatSettings()
        {
            try
            {
                var chatSettingsString = JsonConvert.SerializeObject(_chatSettings, Formatting.Indented);
                File.WriteAllText("chatsettings.json", chatSettingsString);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void SaveBoyans()
        {
            try
            {
                var boyansString = JsonConvert.SerializeObject(_boyans, Formatting.Indented);
                File.WriteAllText("boyans.json", boyansString);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }            
        }

        private static void CleanTemp()
        {
            var tempFolderDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Tmp"));
            if(tempFolderDir.Exists)
            {
                foreach (var tmpFile in tempFolderDir.EnumerateFiles())
                {
                    tmpFile.Delete();
                }
            }
        }

        private static void SetupPeriodicSettingsSaving()
        {
            _saveTimer = new Timer(20 * 60 * 1000);
            _saveTimer.Elapsed += Timer_Elapsed;
            _saveTimer.AutoReset = true;
            _saveTimer.Start();
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SaveBoyans();
            SaveChatSettings();
            CleanTemp();
        }
    }
}
