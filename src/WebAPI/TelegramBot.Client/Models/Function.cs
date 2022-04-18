using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBot.Client.Models
{
    public class Function
    {
        public int FunctionId { get; set; }

        public string FunctionName { get; set; }

        public byte Type { get; set; }

        public bool Status { get; set; }
    }
}