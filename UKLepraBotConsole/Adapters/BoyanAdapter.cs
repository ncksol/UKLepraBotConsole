using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using UKLepraBotConsole.Models;

namespace UKLepraBotConsole.Adapters
{
    public class BoyanAdapter : MessageAdapterBase
    {
        public BoyanAdapter(TelegramBotClient bot, BoyanList boyans) : base(bot, boyans)
        {
        }

        public async override Task Process(Message message)
        {
            HelperMethods.IsUrl(message, out var url);
            var uri = new Uri(url.TrimEnd('/', '?'));

            string cleanUrl;
            if (uri.Host.Contains("youtube.com"))
            {
                cleanUrl = uri.Host.Replace("www.", "") + uri.PathAndQuery;
            }
            else
            {
                cleanUrl = uri.Host.Replace("www.", "") + uri.AbsolutePath;
            }

            var result = Boyans.Items.OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Url.Contains(cleanUrl));
            if (result != null)
            {
                var choice = HelperMethods.RandomInt(5);

                switch (choice)
                {
                    case (0):
                        await Reply(message.Chat.Id, message.MessageId, gif: Gifs.WavingOldLadiesGif);
                        break;
                    case (1):
                        await Reply(message.Chat.Id, message.MessageId, gif: Gifs.SlowpokeGif);
                        break;
                    case (2):
                        await Reply(message.Chat.Id, message.MessageId, sticker: Stickers.SlowpokeWoS);
                        break;
                    case (3):
                        await Reply(message.Chat.Id, message.MessageId, gif: Stickers.SlowpokeGikMe);
                        break;
                    default:
                        await Reply(message.Chat.Id, message.MessageId, text: "Боян!");
                        break;
                }

                await Bot.ForwardMessageAsync(chatId: message.Chat.Id, fromChatId: message.Chat.Id, messageId: Convert.ToInt32(result.MessageId));
            }
            else
            {
                var newBoyan = new Boyan { Url = cleanUrl, MessageId = message.MessageId.ToString(), ChatId = message.Chat.Id.ToString(), DateCreated = DateTimeOffset.UtcNow};
                Boyans.Items.Add(newBoyan);
            }

            Console.WriteLine("Processed Boyan message event");
        }

        private async Task Reply(long chatId, int messageId, string text = "", string sticker = "", string gif = "")
        {
            if(string.IsNullOrEmpty(text) == false)
            {
                await Bot.SendTextMessageAsync(chatId: chatId, replyToMessageId: messageId, text: text);
            }
            else if (string.IsNullOrEmpty(sticker) == false)
            {
                await Bot.SendStickerAsync(chatId: chatId, replyToMessageId: messageId, sticker: sticker);
            }
            else if (string.IsNullOrEmpty(gif) == false)
            {
                await Bot.SendDocumentAsync(chatId: chatId, replyToMessageId: messageId, document: new InputOnlineFile(gif));
            }
        }
    }
}
