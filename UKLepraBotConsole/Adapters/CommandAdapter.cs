using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UKLepraBotConsole.Models;

namespace UKLepraBotConsole
{
    public class CommandAdapter : MessageAdapterBase
    {
        public CommandAdapter(TelegramBotClient bot, ChatSettings chatSettings, BoyanList boyanList) : base(bot, chatSettings, boyanList)
        {
        }

        public override async Task Process(Message message)
        {
            string messageText;
            if (message.Type == MessageType.Photo)
                messageText = message.Caption;
            else
                messageText = message.Text;
            
            var conversationId = message.Chat.Id.ToString();

            var delaySettings = ChatSettings.DelaySettings.ContainsKey(conversationId)
                ? ChatSettings.DelaySettings[conversationId]
                : null;
            var state = ChatSettings.State.ContainsKey(conversationId)
                ? ChatSettings.State[conversationId]
                : (bool?)null;
            var currentDelay = ChatSettings.Delay.ContainsKey(conversationId)
                ? ChatSettings.Delay[conversationId]
                : (int?)null;

            string reply = null;

            if (messageText.ToLower().Contains("/huify"))
                reply = StartHuifyCommand(message, conversationId, delaySettings);
            else if (messageText.ToLower().Contains("/unhuify"))
                reply = StopHuifyCommand(message, conversationId);
            else if (messageText.ToLower().Contains("/status"))
                reply = StatusCommand(state, currentDelay, delaySettings);
            else if (messageText.ToLower().Contains("/uptime"))
                reply = UptimeCommand();
            else if (messageText.ToLower().Contains("/delay"))
                reply = DelayCommand(message);
            else if (messageText.ToLower().Contains("/secret"))
                reply = SecretCommand(message);
            else if (messageText.ToLower().Contains("/ban"))
                reply = await BanCommand(message);
            else if(messageText.ToLower().Contains("/version"))
                reply = VersionCommand();
            else if (messageText.ToLower().Contains("/reload"))
            {
                ReloadReactionsCommand();
                return;
            }

            if(string.IsNullOrEmpty(reply)) return;

            await Bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: reply);

            Console.WriteLine("Processed Command message event");
        }

        private void ReloadReactionsCommand()
        {
            //ReactionsManager.ReloadReactions();
        }

        private string DelayCommand(Message message)
        {
            var reply = string.Empty;

            if (VerifyAdminCommandAccess(message) == false)
            {
                reply = GetAcccessDeniedCommandText();
                return reply;
            }

            var messageText = message.Text;
            var conversationId = message.Chat.Id.ToString();
            var delaySettings = ChatSettings.DelaySettings;

            var messageParts = messageText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (messageParts.Length == 1)
            {
                var currentDelay = new Tuple<int, int>(0, 4);
                if (delaySettings.ContainsKey(conversationId))
                    currentDelay = delaySettings[conversationId];

                reply = $"Сейчас я пропускаю случайное число сообщений от {currentDelay.Item1} до {currentDelay.Item2}";
            }
            else if (messageParts.Length == 2)
            {
                int newMaxDelay;
                if (!int.TryParse(messageParts[1], out newMaxDelay))
                {
                    reply = "Неправильный аргумент, отправьте /delay N [[M]], где N, M любое натуральное число";
                }
                else
                {
                    delaySettings[conversationId] = new Tuple<int, int>(0, newMaxDelay);
                    reply = $"Я буду пропускать случайное число сообщений от 0 до {newMaxDelay}";
                }
            }
            else if (messageParts.Length == 3)
            {
                int newMaxDelay;
                int newMinDelay;
                if (!int.TryParse(messageParts[2], out newMaxDelay))
                {
                    reply = "Неправильный аргумент, отправьте /delay N [[M]], где N, M любое натуральное число";
                }
                else if (!int.TryParse(messageParts[1], out newMinDelay))
                {
                    reply = "Неправильный аргумент, отправьте /delay N [[M]], где N, M любое натуральное число";
                }
                else
                {
                    if (newMinDelay == newMaxDelay)
                    {
                        newMinDelay = 0;
                    }
                    else if (newMinDelay > newMaxDelay)
                    {
                        var i = newMinDelay;
                        newMinDelay = newMaxDelay;
                        newMaxDelay = i;
                    }

                    ChatSettings.DelaySettings[conversationId] = new Tuple<int, int>(newMinDelay, newMaxDelay);
                    reply = $"Я буду пропускать случайное число сообщений от {newMinDelay} до {newMaxDelay}";
                }
            }

            return reply;
        }

        private static string UptimeCommand()
        {
            var uptime = DateTimeOffset.UtcNow - Configuration.StartupTime.Value;
            var reply =
                $"Uptime: {(int)uptime.TotalDays} days, {uptime.Hours} hours, {uptime.Minutes} minutes, {uptime.Seconds} seconds.";
            return reply;
        }
        
        private static string VersionCommand()
        {
            return Configuration.Version;
        }

        private static string StatusCommand(bool? state, int? currentDelay, Tuple<int, int> delaySettings)
        {
            string reply;

            if (!state.HasValue)
                reply = "Хуятор не инициализирован." + Environment.NewLine;
            else if (state.Value)
                reply = "Хуятор активирован." + Environment.NewLine;
            else
                reply = "Хуятор не активирован." + Environment.NewLine;

            if (!currentDelay.HasValue)
                reply += "Я не знаю когда отреагирую в следующий раз." + Environment.NewLine;
            else
                reply += $"В следующий раз я отреагирую через {currentDelay.Value} сообщений." +
                              Environment.NewLine;

            if (delaySettings == null)
                reply += "Настройки задержки не найдены. Использую стандартные от 0 до 4 сообщений.";
            else
                reply +=
                    $"Сейчас я пропускаю случайное число сообщений от {delaySettings.Item1} до {delaySettings.Item2}";
            return reply;
        }

        private string StopHuifyCommand(Message message, string conversationId)
        {
            string reply;

            if (VerifyAdminCommandAccess(message) == false)
            {
                reply = GetAcccessDeniedCommandText();
                return reply;
            }

            reply = "Хуятор успешно деактивирован.";
            ChatSettings.State[conversationId] = false;
            return reply;
        }

        private bool VerifyAdminCommandAccess(Message message)
        {
            var masterId = Configuration.MasterId.ToString();
            var adminIds = Configuration.AdminIds?.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            adminIds.Add(masterId);

            return adminIds.Contains(message.From.Id.ToString());
        }

        private string GetAcccessDeniedCommandText()
        {
            return "Не положено холопам королеве указывать!";
        }

        private string StartHuifyCommand(Message message, string conversationId, Tuple<int, int> delaySettings)
        {
            string reply;

            if (VerifyAdminCommandAccess(message) == false)
            {
                reply = GetAcccessDeniedCommandText();
                return reply;
            }

            reply = "Хуятор успешно активирован.";
            ChatSettings.State[conversationId] = true;

            if (delaySettings != null)
                ChatSettings.Delay[conversationId] = Rnd.Next(delaySettings.Item1, delaySettings.Item2 + 1);
            else
                ChatSettings.Delay[conversationId] = Rnd.Next(4);
            return reply;
        }

        private string SecretCommand(Message message)
        {
            var messageText = message.Text;
            var messageParts = messageText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var secretKey = Configuration.SecretKey;
            if (messageParts[1] != secretKey) return null;

            var secretMessage = string.Join(" ", messageParts.Skip(2));

            return secretMessage;
        }

        private async Task<string> BanCommand(Message message)
        {
            var reply = string.Empty;

            if (VerifyAdminCommandAccess(message) == false)
            {
                reply = GetAcccessDeniedCommandText();
                return reply;
            }

            if (message.Type == MessageType.Text)
                reply = BanUrlBoyan(message);
            else if (message.Type == MessageType.Photo)
                reply = await BanImageBoyan(message);
            
            return reply;
        }


        private string BanUrlBoyan(Message message)
        {
            var text = message.Text;
            var isBanning = IsBanning(text, message);
            if (isBanning == false) return null;
            
            text = text.Substring($"/ban@{Configuration.TelegramBotId}".Length).Trim();

            if(HelperMethods.IsUrl(text, out var url) == false) return null;

            var uri = new Uri(url.TrimEnd('/', '?'));

            string cleanUrl;
            if (uri.Host.Contains("youtube.com"))
            {
                cleanUrl = uri.Host.Replace("www.", "") + uri.PathAndQuery;
            }
            else
            {
                cleanUrl = uri.Host.Replace("www.", "") + uri.AbsolutePath;
            }

            string reply;
            var boyan = Boyans.Items.Where(x => x.Url != null).OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Url.Contains(cleanUrl));
            if(boyan != null)
            {
                boyan.IsBanned = true;
                reply = "Небоян забанен!";
            }
            else
            {
                reply = "Боян не найден!";
            }

            return reply;
        }

        private async Task<string> BanImageBoyan(Message message)
        {
            var isBanning = IsBanning(message.Caption, message);
            if(isBanning == false) return null;

            var biggestPhoto = message.Photo.OrderByDescending(x => x.FileSize).FirstOrDefault();

            var tempFolderPath = Path.Combine(AppContext.BaseDirectory, "Tmp");
            if (Directory.Exists(tempFolderPath) == false)
                Directory.CreateDirectory(tempFolderPath);

            var filePath = Path.Combine(tempFolderPath, $"{Guid.NewGuid()}.jpg");
            using (var stream = System.IO.File.Create(filePath))
            {
                var file = await Bot.GetInfoAndDownloadFileAsync(biggestPhoto.FileId, stream);
            }

            var hash = new ImgHash();
            hash.GenerateFromPath(filePath);

            Boyan boyan = null;
            foreach (var imageBoyan in Boyans.Items.Where(x => x.ImageHash != null))
            {
                var similarity = hash.CompareWith(imageBoyan.ImageHash);
                if (similarity >= 99)
                {
                    boyan = imageBoyan;
                    break;
                }
            }

            string reply;
            if (boyan != null)
            {
                boyan.IsBanned = true;
                reply = "Небоян забанен!";
            }
            else
            {
                reply = "Боян не найден!";
            }

            return reply;
        }

        private bool IsBanning(string text, Message message)
        {
            return text.StartsWith("/ban") && message.From.Id == Configuration.MasterId;
        }
    }
}
