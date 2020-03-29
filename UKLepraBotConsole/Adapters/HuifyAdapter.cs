using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using UKLepraBotConsole.Models;

namespace UKLepraBotConsole
{
    public class HuifyAdapter : MessageAdapterBase
    {
        public HuifyAdapter(TelegramBotClient bot, ChatSettings chatSettings) : base(bot, chatSettings)
        {
        }

        public async override Task Process(Message message)
        {
            if(ShouldProcessMessage(message) == false) return;
            
            var messageText = message.Text;
            var huifiedMessage = Huify.HuifyMe(messageText);
            if (string.IsNullOrEmpty(huifiedMessage)) return;

            await Bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        text: huifiedMessage);

            Console.WriteLine("Processed Huify message event");
        }


        private bool ShouldProcessMessage(Message message)
        {
            var rnd = new Random();
            var conversationId = message.Chat.Id.ToString();

            var state = ChatSettings.State;
            var delay = ChatSettings.Delay;
            var delaySettings = ChatSettings.DelaySettings;
            if (!state.ContainsKey(conversationId) || !state[conversationId])//huify is not active or was never activated
                return false;

            var shouldProcessMessage = false;
            var resetDelay = false;
            if (delay.ContainsKey(conversationId))
            {
                if (delay[conversationId] > 0)
                {
                    delay[conversationId] -= 1;
                }
                else if (delay[conversationId] == 0 && message.From.Id != Configuration.MasterId && string.IsNullOrEmpty(message.Text) == false)
                {
                    shouldProcessMessage = true;
                }
            }
            else
            {
                resetDelay = true;
            }

            if (resetDelay || shouldProcessMessage)
            {
                Tuple<int, int> delaySetting;
                if (delaySettings.TryGetValue(conversationId, out delaySetting))
                    delay[conversationId] = rnd.Next(delaySetting.Item1, delaySetting.Item2 + 1);
                else
                    delay[conversationId] = rnd.Next(4);
            }

            return shouldProcessMessage;
        }
    }
}
