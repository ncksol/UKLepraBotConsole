using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace UKLepraBotConsole.Adapters
{
    public class StickerAdapter : MessageAdapterBase
    {
        public StickerAdapter(TelegramBotClient bot) : base(bot)
        {
        }

        public async override Task Process(Message message)
        {
            await Bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        text: message.Sticker?.FileId ?? message.Animation?.FileId);
        }
    }
}
