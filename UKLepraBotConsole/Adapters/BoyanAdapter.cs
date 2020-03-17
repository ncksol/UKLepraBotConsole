using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using UKLepraBotConsole.Models;
using File = System.IO.File;

namespace UKLepraBotConsole.Adapters
{
    public class BoyanAdapter : MessageAdapterBase
    {
        public BoyanAdapter(TelegramBotClient bot, BoyanList boyans) : base(bot, boyans)
        {
        }

        public async override Task Process(Message message)
        {
            Boyan boyan;

            if (message.Type == MessageType.Text)
                boyan = ProcessUrlBoyan(message);
            else if(message.Type == MessageType.Photo)
                boyan = await ProcessImageBoyan(message);
            else return;

            if(boyan != null)
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

                await Bot.ForwardMessageAsync(chatId: message.Chat.Id, fromChatId: message.Chat.Id, messageId: Convert.ToInt32(boyan.MessageId));
            }

            Console.WriteLine("Processed Boyan message event");
        }

        private Boyan ProcessUrlBoyan(Message message)
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

            var boyan = Boyans.Items.Where(x => x.Url != null).OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Url.Contains(cleanUrl));

            if(boyan == null)
            {
                var newBoyan = new Boyan { Url = cleanUrl, MessageId = message.MessageId.ToString(), ChatId = message.Chat.Id.ToString(), DateCreated = DateTimeOffset.UtcNow };
                Boyans.Items.Add(newBoyan);
            }

            return boyan;
        }

        private async Task<Boyan> ProcessImageBoyan(Message message)
        {
            var biggestPhoto = message.Photo.OrderByDescending(x => x.FileSize).FirstOrDefault();

            var tempFolderPath = Path.Combine(AppContext.BaseDirectory, "Tmp");
            if(Directory.Exists(tempFolderPath) == false)
                Directory.CreateDirectory(tempFolderPath);

            var filePath = Path.Combine(tempFolderPath, $"{Guid.NewGuid()}.jpg");
            using (var stream = File.Create(filePath))
            {
                var file = await Bot.GetInfoAndDownloadFileAsync(biggestPhoto.FileId, stream);
            }

            var hash = new ImgHash(16);
            hash.GenerateFromPath(filePath);

            Boyan boyan = null;
            foreach (var imageBoyan in Boyans.Items.Where(x => x.ImageHash != null))
            {
                var similarity = hash.CompareWith(imageBoyan.ImageHash);
                if (similarity >= 80)
                {
                    boyan = imageBoyan;
                    break;
                }
            }

            if(boyan == null)
            {
                var newBoyan = new Boyan { ImageHash = hash.HashData, MessageId = message.MessageId.ToString(), ChatId = message.Chat.Id.ToString(), DateCreated = DateTimeOffset.UtcNow };
                Boyans.Items.Add(newBoyan);
            }

            return boyan;
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
