using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace UKLepraBotConsole
{
    public class HelperMethods
    {
        public static bool YesOrNo()
        {
            var rnd = new Random();
            return rnd.Next() % 2 == 0;
        }

        public static int RandomInt(int max)
        {
            var rnd = new Random();
            return rnd.Next(max);
        }

        public static bool MentionsId(Message message, string id)
        {
            //var channelData = (JObject)message.ChannelData;
            //var messageData = JsonConvert.DeserializeObject<JsonModels.Message>(channelData["message"].ToString());

            //if (messageData?.reply_to_message?.@from?.username == WebApiApplication.TelegramBotName)
            //    return true;

            if (string.IsNullOrEmpty(message.Text)) return false;

            return message.Text.Contains($"@{id}");
        }

        public static bool MentionsBot(Message message)
        {
            return MentionsId(message, Configuration.TelegramBotId);
        }
    }
}
