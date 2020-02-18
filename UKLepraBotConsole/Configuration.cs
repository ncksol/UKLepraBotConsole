using System;
using System.Collections.Generic;
using System.Text;

namespace UKLepraBotConsole
{
    public static class Configuration
    {        
        public readonly static string BotToken = "{TELEGRAM BOT TOKEN}";
        public readonly static string SecretKey = "{TELEGRAM BOT SECRET KEY}";
        public readonly static string TelegramBotId = "ukleprabot";
        public readonly static int MasterId = 178846839;
        public readonly static string AdminIds = "118698210";

        private static DateTimeOffset? _startupTime = null;
        public static DateTimeOffset? StartupTime
        {
            get => _startupTime;
            set
            {
                if(_startupTime == null)
                    _startupTime = value;
            }
        }
    }
}
