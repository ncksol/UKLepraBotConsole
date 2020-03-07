using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using UKLepraBotConsole.Models;

namespace UKLepraBotConsole
{
    public class MessageAdapterBase
    {
        protected Random Rnd;
        protected TelegramBotClient Bot;
        protected ChatSettings ChatSettings;
        protected ReactionsList Reactions;
        protected BoyanList Boyans;

        protected MessageAdapterBase(TelegramBotClient bot)
        {
            Bot = bot;
            Rnd = new Random();
        }
        
        protected MessageAdapterBase(TelegramBotClient bot, ChatSettings chatSettings):this(bot)
        {
            ChatSettings = chatSettings;
        }
        
        protected MessageAdapterBase(TelegramBotClient bot, BoyanList boyans):this(bot)
        {
            Boyans = boyans;
        }

        public virtual Task Process(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
