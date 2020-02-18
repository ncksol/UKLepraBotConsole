using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace UKLepraBotConsole
{
    public class ReactionAdapter : MessageAdapterBase
    {
        private readonly Reaction _reaction;

        public ReactionAdapter(TelegramBotClient bot, Reaction reaction) : base(bot)
        {
            _reaction = reaction;
        }

        public async override Task Process(Message message)
        {
            var reactionReply = _reaction.Replies.Count <= 1 ? _reaction.Replies.FirstOrDefault() : _reaction.Replies[HelperMethods.RandomInt(_reaction.Replies.Count)];

            if(reactionReply == null) return;

            if(string.IsNullOrEmpty(reactionReply.Text) == false)
            {
                await Bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            text: reactionReply.Text);
            }
            
            if(string.IsNullOrEmpty(reactionReply.Sticker) == false)
            {
                await Bot.SendStickerAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            sticker: reactionReply.Sticker);
            }

            Console.WriteLine("Processed Reaction event");
        }
    }
}
