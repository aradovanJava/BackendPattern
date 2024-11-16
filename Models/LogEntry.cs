using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace front.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string LogLevel { get; set; }
        public string? By { get; set; }
        public string Action { get; set; }
    }

}
