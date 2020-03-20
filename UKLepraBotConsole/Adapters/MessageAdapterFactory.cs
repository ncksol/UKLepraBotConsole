using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UKLepraBotConsole.Models;

namespace UKLepraBotConsole.Adapters
{
    public class MessageAdapterFactory
    {
        public static readonly List<string> CommandAdapterActivators = new List<string> { "/status", "/huify", "/unhuify", "/uptime", "/delay", "/secret", "/reload", "/sticker", "/ban" };
        public static readonly List<string> AIAdapterActivators = new List<string> { "погугли" };
        private readonly TelegramBotClient _bot;
        private readonly ChatSettings _chatSettings;
        private readonly ReactionsList _reactions;
        private readonly BoyanList _boyans;

        public MessageAdapterFactory(TelegramBotClient bot, ChatSettings chatSettings, ReactionsList reactions, BoyanList boyans)
        {
            _bot = bot;
            _chatSettings = chatSettings;
            _reactions = reactions;
            _boyans = boyans;
        }

        public MessageAdapterBase CreateAdapter(Message message)
        {
            if (message.Type == MessageType.Text && HelperMethods.MentionsBot(message) && !string.IsNullOrEmpty(message.Text) && CommandAdapterActivators.Any(x => message.Text.ToLower().Contains(x)))
                return new CommandAdapter(_bot, _chatSettings, _boyans);

            if (message.Type == MessageType.Photo && !string.IsNullOrEmpty(message.Caption) && CommandAdapterActivators.Any(x => message.Caption.ToLower().Contains(x)))
                return new CommandAdapter(_bot, _chatSettings, _boyans);

            if (message.Type == MessageType.Text && !string.IsNullOrEmpty(message.Text) && AIAdapterActivators.Any(x => message.Text.ToLower().Contains(x)))
                return new AIAdapter(_bot);

            if (message.ReplyToMessage == null && ((message.Type == MessageType.Text && HelperMethods.IsUrl(message.Text, out _)) || (message.Type == MessageType.Photo && message.Photo != null)))
                return new BoyanAdapter(_bot, _boyans);

            if (message.Chat.Type == ChatType.Private && message.From.Id == Configuration.MasterId && (message.Sticker != null || message.Animation != null))
                return new StickerAdapter(_bot);

            if (IsReactionAdapterMessage(message, out var reaction))
                return new ReactionAdapter(_bot, reaction);

            return new HuifyAdapter(_bot, _chatSettings);
        }

        private bool IsReactionAdapterMessage(Message message, out Reaction reaction)
        {
            var messageText = message.Text?.ToLower() ?? string.Empty;

            reaction = _reactions.Items.FirstOrDefault(x => x.Triggers.Any(messageText.Contains));

            if (reaction == null) return false;
            if (reaction.IsMentionReply && HelperMethods.MentionsBot(message) == false) return false;
            if (reaction.IsAlwaysReply == false && HelperMethods.YesOrNo() == false) return false;

            return true;
        }

    }
}
