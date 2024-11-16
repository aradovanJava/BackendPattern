using front.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Back.Controllers
{
    [Route("logs")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private static List<string> _logFiles;
        public LogsController()
        {
            GetListOfLogFiles();
        }

        [HttpGet]
        public async Task<ActionResult<LogEntry>> GetLatestLogEntry()
        {
            foreach (var logFile in _logFiles)
            {
                var logEntries = await ReadLogEntries(logFile);
                if (logEntries.Any())
                {
                    return Ok(logEntries);
                }
            }

            return NotFound("No log entries found.");
        }

        private async Task<List<LogEntry>> ReadLogEntries(string filePath)
        {
            var logEntries = new List<LogEntry>();

            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var logEntry = ParseLogEntry(line);
                    if (logEntry != null)
                    {
                        logEntries.Add(logEntry);
                    }
                }
            }

            return logEntries.OrderByDescending(e => e.Timestamp).ToList();
        }

        private LogEntry ParseLogEntry(string logLine)
        {
            var logPattern = @"^(?<timestamp>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} \+\d{2}:\d{2}) \[(?<logLevel>[A-Z]+)\] By:(?<by>[^ ]*)\s+Action:(?<action>.*)$";
            var match = Regex.Match(logLine, logPattern);

            if (match.Success)
            {
                return new LogEntry
                {
                    Timestamp = DateTime.ParseExact(match.Groups["timestamp"].Value, "yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture),
                    LogLevel = match.Groups["logLevel"].Value,
                    By = match.Groups["by"].Value,
                    Action = match.Groups["action"].Value
                };
            }

            return null;
        }

        public static void GetListOfLogFiles() =>
            _logFiles = Directory.GetFiles(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Logs"), "Logs*.txt")
                            .OrderByDescending(f => f)
                            .Skip(1)
                            .ToList();
    }
}