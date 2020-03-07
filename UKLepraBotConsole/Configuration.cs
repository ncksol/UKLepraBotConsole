using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UKLepraBotConsole
{
    public static class Configuration
    {        
        
        public readonly static int MasterId = 178846839;
        public readonly static string AdminIds = "118698210";

        public static string TelegramBotId { get; set; } = "ukleprabot";

        private static string _botToken;
        public static string BotToken
        {
            get
            {
                if(string.IsNullOrEmpty(_botToken))
                    _botToken = ReadToken("bot.token");

                return _botToken;
            }
            set
            {
                _botToken = value;
            }
        }

        private static string _secretKey;
        public static string SecretKey
        {
            get
            {
                if (string.IsNullOrEmpty(_secretKey))
                    _secretKey = ReadToken("secret.key");

                return _secretKey;
            }
            set
            {
                _secretKey = value;
            }
        }


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

        private static string ReadToken(string fileName)
        {
            var file = new FileInfo(fileName);
            if (file.Exists == false) return string.Empty;

            using (var reader = file.OpenText())
                return reader.ReadToEnd();
        }
    }
}
