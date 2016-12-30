using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMvc.Models
{
    public class LogEvent
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public int Type { get; set; }
        public string ShortInfo { get; set; }
        public string MoreInfo { get; set; }
    }
}