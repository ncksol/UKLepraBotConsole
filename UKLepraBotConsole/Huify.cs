using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UKLepraBotConsole
{
    public class Huify
    {
        private static Random _rnd = new Random();


        private static List<string> _suggestions = new List<string>
        {
            "Возможно, вы имели ввиду: ",
            "Все свои люди, ",
            "Давай прямо, ",
            "Давайте называть вещи своими именами, ",
            "Другими словами, ",
            "Извините, ",
            "Извините, но ",
            "Или, как я бы сказал, ",
            "Не стесняйся, ",
            "Ну как бы, ",
            "Подождите, ",
            "Поправочка, ",
            "Простите, ",
            "Чего уж скрывать, ",
            "Я бы сказал, ",
        };


        public static string HuifyMe(string message)
        {
            var huifiedMessage = HuifyMeInternal(message);
            if (String.IsNullOrEmpty(huifiedMessage)) return String.Empty;

            return huifiedMessage;
        }


        private static string HuifyMeInternal(string message)
        {
            var vowels = "оеаяуюы";
            var rulesValues = "еяюи";
            var rules = new Dictionary<string, string>
            {
                {"о", "е"},
                {"а", "я"},
                {"у", "ю"},
                {"ы", "и"}
            };
            var nonLettersPattern = new Regex("[^а-яё-]+");
            var onlyDashesPattern = new Regex("^-*$");
            var prefixPattern = new Regex("^[бвгджзйклмнпрстфхцчшщьъ]+");

            var messageParts = message.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (messageParts.Length < 1) return String.Empty;

            var word = messageParts[messageParts.Length - 1];
            var prefix = string.Empty;

            if (messageParts.Length > 1 && _rnd.Next(0, 1) == 1)
            {
                prefix = messageParts[messageParts.Length - 2];
            }

            word = nonLettersPattern.Replace(word.ToLower(), "");
            if (word == "бот")
            {
                return (string.IsNullOrEmpty(prefix) ? "" : prefix + " ") + "хуебот";
            }

            if (onlyDashesPattern.IsMatch(word))
                return string.Empty;

            var postFix = prefixPattern.Replace(word, "");
            if (postFix.Length < 3) return string.Empty;

            var foo = postFix.Substring(1, 1);
            if (word.Substring(2) == "ху" && rulesValues.Contains(foo))
            {
                return string.Empty;
            }

            if (rules.ContainsKey(postFix.Substring(0, 1)))
            {
                if (!vowels.Contains(foo))
                {
                    return (string.IsNullOrEmpty(prefix) ? "" : prefix + " ") + "ху" + rules[postFix.Substring(0, 1)] + postFix.Substring(1);
                }
                else
                {
                    if (rules.ContainsKey(foo))
                    {
                        return (string.IsNullOrEmpty(prefix) ? "" : prefix + " ") + "ху" + rules[foo] + postFix.Substring(2);
                    }
                    else
                    {
                        return (string.IsNullOrEmpty(prefix) ? "" : prefix + " ") + "ху" + postFix.Substring(1);
                    }
                }
            }
            else
            {
                return (string.IsNullOrEmpty(prefix) ? "" : prefix + " ") + "ху" + postFix;
            }
        }
    }
}
