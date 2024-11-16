using Back.Models;
using Serilog;

namespace front.Utils.Logger
{
    public sealed class Logger
    {
        private static readonly object _lock = new object();
        private static Logger _instance;

        private Logger()
        {
            string storagePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Logs\\Logs.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(storagePath, rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();
        }

        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Logger();
                        }
                    }
                }
                return _instance;
            }
        }

        public void Information(string message, int? user = null) =>
            Log.Information($"By:{user}\tAction:{message}");

        public void Warning(string message, int? user = null) =>
            Log.Warning(user == null ? $"Action:{message}" : $"By:{user}\tAction:{message}");

        public void Error(string message, int? user = null) =>
            Log.Error($"By:{user}\tAction:{message}");
    }

}
