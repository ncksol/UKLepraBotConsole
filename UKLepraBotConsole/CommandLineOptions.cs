using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace UKLepraBotConsole
{
    class CommandLineOptions
    {
        [Option("token", Required = false, HelpText = "Bot Token.")]
        public string BotToken { get; set; }
        
        [Option("botid", Required = false, HelpText = "Bot Id.")]
        public string BotId { get; set; }

        [Option("secret", Required = false, HelpText = "Secret Key.")]
        public string SecretKey { get; set; }

        [Option("service", Required = false, HelpText = "Run as a Service.")]
        public bool IsService { get; set; }
    }
}
