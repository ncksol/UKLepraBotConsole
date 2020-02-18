using System;
using System.Collections.Generic;
using System.Text;

namespace UKLepraBotConsole
{
    public class Reaction
    {
        public List<string> Triggers { get; set; } = new List<string>();
        public List<Reply> Replies { get; set; } = new List<Reply>();
        public bool IsAlwaysReply { get; set; }
        public bool IsMentionReply { get; set; }
    }

    public class ReactionsList
    {
        public List<Reaction> Items { get; set; }
    }

    public class Reply
    {
        public string Text { get; set; }
        public string Sticker { get; set; }
    }
}
