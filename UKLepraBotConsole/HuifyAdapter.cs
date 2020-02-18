using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace UKLepraBotConsole
{
    public class HuifyAdapter : MessageAdapterBase
    {
        public HuifyAdapter(TelegramBotClient bot, ChatSettings chatSettings) : base(bot, chatSettings)
        {
        }

        public async override Task Process(Message message)
        {
            if (message.From.Id == Configuration.MasterId)
                return;

            var messageText = message.Text;
            var conversationId = message.Chat.Id.ToString();

            var state = ChatSettings.State;
            var delay = ChatSettings.Delay;
            var delaySettings = ChatSettings.DelaySettings;
            if (!state.ContainsKey(conversationId) || !state[conversationId])
                return;

            if (delay.ContainsKey(conversationId))
            {
                delay[conversationId] -= 1;
            }
            else
            {
                Tuple<int, int> delaySetting;
                if (delaySettings.TryGetValue(conversationId, out delaySetting))
                    delay[conversationId] = Rnd.Next(delaySetting.Item1, delaySetting.Item2 + 1);
                else
                    delay[conversationId] = Rnd.Next(4);
            }

            if (delay[conversationId] != 0) return;

            delay.Remove(conversationId);
            var huifiedMessage = Huify.HuifyMe(messageText);
            if (string.IsNullOrEmpty(huifiedMessage)) return;

            await Bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        text: huifiedMessage);

            Console.WriteLine("Processed Huify message event");
        }
    }
}
