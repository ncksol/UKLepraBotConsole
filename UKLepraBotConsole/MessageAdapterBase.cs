using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace UKLepraBotConsole
{
    public class MessageAdapterBase
    {
        protected Random Rnd;
        protected TelegramBotClient Bot;
        protected ChatSettings ChatSettings;
        protected ReactionsList Reactions;

        protected MessageAdapterBase(TelegramBotClient bot)
        {
            Bot = bot;
            Rnd = new Random();
        }
        
        protected MessageAdapterBase(TelegramBotClient bot, ChatSettings chatSettings):this(bot)
        {
            ChatSettings = chatSettings;
        }

        public virtual Task Process(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
