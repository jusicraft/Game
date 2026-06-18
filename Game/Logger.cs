using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Game
{
    public static class Logger
    {
        private static readonly Channel<string> _channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true
        });

        private static readonly string LogFilePath = "server.log";

        static Logger()
        {
            Task.Run(ProcessQueueAsync);
        }

        public static void Log(string message)
        {
            _channel.Writer.TryWrite($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }

        private static async Task ProcessQueueAsync()
        {
            try
            {
                using var writer = new StreamWriter(LogFilePath, true) { AutoFlush = true };
                while (await _channel.Reader.WaitToReadAsync())
                {
                    while (_channel.Reader.TryRead(out string? message))
                    {
                        await writer.WriteLineAsync(message);
                        Console.WriteLine(message);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
