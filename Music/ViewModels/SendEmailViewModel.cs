using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music.ViewModels
{
    public class SendEmailViewModel
    {
        
        public string RecepientId { get; set; }
        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public SendEmailViewModel() {}
        public SendEmailViewModel(string recepientId , string from, string to)
        {
            RecepientId = recepientId;
            EmailFrom = from;
            EmailTo = to;
        }
    }
}
