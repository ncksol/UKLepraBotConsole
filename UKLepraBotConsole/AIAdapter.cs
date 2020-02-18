using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace UKLepraBotConsole
{
    public class AIAdapter : MessageAdapterBase
    {
        private string[] _rubbish = new[] { ".", ",", "-", "=", "#", "!", "?", "%", "@", "\"", "£", "$", "^", "&", "*", "(", ")", "_", "+", "]", "[", "{", "}", ";", ":", "~", "/", "<", ">", };

        public AIAdapter(TelegramBotClient bot):base(bot)
        {
        }

        public override async Task Process(Message message)
        {
            var messageText = message.Text?.ToLower() ?? string.Empty;

            if (messageText.ToLower().Contains("погугли"))
            {
                await GoogleCommand(message);
                return;
            }
        }

        private async Task GoogleCommand(Message message)
        {
            var messageText = message.Text;

            var activationWord = "погугли";

            var cleanedMessageText = messageText;
            _rubbish.ToList().ForEach(x => cleanedMessageText = cleanedMessageText.Replace(x, " "));

            var messageParts = cleanedMessageText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var activationWordPosition = messageParts.FindIndex(x => x.Equals(activationWord));
            if (activationWordPosition == -1 || activationWordPosition > 3) return;

            var queryParts = messageParts.Skip(activationWordPosition + 1);
            if (!queryParts.Any()) return;

            var query = string.Join("%20", queryParts);
            var reply = $"[Самому слабо было?](http://google.co.uk/search?q={query})";

            await Bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        text: reply,
                        disableWebPagePreview: true,
                        parseMode: ParseMode.MarkdownV2);

            Console.WriteLine("Processed GoogleIt event");
        }
    }
}
