using System;
using System.Collections.Generic;
using System.Text;

namespace UKLepraBotConsole.Models
{
    public class Boyan
    {
        public string ImageHash { get;set;}
        public string Url { get;set;}
        public string ChatId { get;set;}
        public string MessageId { get;set;}
        public DateTimeOffset DateCreated { get;set;}
        public bool IsBanned { get; set; }
    }

    public class BoyanList
    {
        public List<Boyan> Items { get;set;} = new List<Boyan>();
    }
}
