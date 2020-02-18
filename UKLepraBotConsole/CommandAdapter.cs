using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace UKLepraBotConsole
{
    public class CommandAdapter : MessageAdapterBase
    {
        public CommandAdapter(TelegramBotClient bot, ChatSettings chatSettings) : base(bot, chatSettings)
        {
        }

        public override async Task Process(Message message)
        {
            var messageText = message.Text;
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
    }
}
