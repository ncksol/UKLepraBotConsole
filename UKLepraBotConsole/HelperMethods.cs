﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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

        public static bool IsUrl(Message message, out string url)
        {
            var rgx = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)");

            var match = rgx.Match(message.Text);
            url = match.ToString();

            return match.Success;
        }

        public static string ReadToken(string fileName)
        {
            var file = new FileInfo(fileName);
            if (file.Exists == false) return string.Empty;

            using (var reader = file.OpenText())
                return reader.ReadToEnd();
        }

    }
}
