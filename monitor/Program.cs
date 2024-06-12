using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace monitor
{
    public class Program
    {
        private const char exitKey = 'q';
        private static System.Timers.Timer? monitorTimer;
        private static string processName = string.Empty;
        private static int lifeTime = 0;
        private static CancellationTokenSource? cts;
        private static CancellationToken token;

        static async Task Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid input!");
                return;
            }

            processName = args[0];
            if (!int.TryParse(args[1], out lifeTime))
            {
                Console.WriteLine("Invalid lifetime value.");
                return;
            }

            if (!int.TryParse(args[2], out int freq))
            {
                Console.WriteLine("Invalid frequency value.");
                return;
            }

            var cts = new CancellationTokenSource();
            var token = cts.Token;

            await RunMonitor(processName, lifeTime, token);

            monitorTimer = new System.Timers.Timer(freq * 60 * 1000);
            monitorTimer.Elapsed += async (sender, e) => await RunMonitor(processName, lifeTime, token);
            monitorTimer.Start();

            await ListenForExitKey(cts);

            monitorTimer.Stop();
            monitorTimer.Elapsed -= OnMonitorElapsed;
            monitorTimer.Dispose();

            Console.WriteLine("Exiting...");
        }
        public static async void OnMonitorElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            await RunMonitor(processName, lifeTime, token);
        }
        public static async Task RunMonitor(string processName, int lifeTime, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            Console.WriteLine("===================================================================");
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            Console.WriteLine("NEW PROCESS CHECK");
            Console.WriteLine("===================================================================");

            var processes = Process.GetProcessesByName(processName);
            string logs = string.Empty;

            if (processes.Length == 0)
            {
                Console.WriteLine($"\nNo processes named '{processName}' are currently running.\n");
            }
            else
            {
                foreach (var process in processes)
                {
                    logs += CheckProcess(process, lifeTime);
                }
            }

            if (!string.IsNullOrEmpty(logs))
            {
                await Task.Run(() => LogToFile(logs));
            }
        }

        public static async Task ListenForExitKey(CancellationTokenSource cts)
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).KeyChar == exitKey)
                {
                    cts.Cancel();
                }
                await Task.Delay(50);
            }
        }

        public static string CheckProcess(Process process, int lifeTime)
        {
            TimeSpan runtime = DateTime.Now - process.StartTime;

            if (process.Responding)
            {
                Console.WriteLine($"\nProcess Information - {process.ProcessName} (PID: {process.Id})");
                Console.WriteLine("--------------------------------------------------");

                Console.WriteLine($"   Status                    : Running");
                Console.WriteLine($"   Runtime                   : {runtime.TotalMinutes:F2} minutes");
                Console.WriteLine($"   Physical memory usage     : {process.WorkingSet64 / 1024 / 1024} MB");
                Console.WriteLine($"   Base priority             : {process.BasePriority}");
                Console.WriteLine($"   Priority class            : {process.PriorityClass}");
                Console.WriteLine($"   User processor time       : {process.UserProcessorTime}");
                Console.WriteLine($"   Privileged processor time : {process.PrivilegedProcessorTime}");
                Console.WriteLine($"   Total processor time      : {process.TotalProcessorTime}");
                Console.WriteLine($"   Paged system memory size  : {process.PagedSystemMemorySize64 / 1024} KB");
                Console.WriteLine($"   Paged memory size         : {process.PagedMemorySize64 / 1024} KB");
            }

            if (runtime.TotalMinutes > lifeTime)
            {
                return KillProcess(process, runtime);
            }

            return string.Empty;
        }

        public static string KillProcess(Process process, TimeSpan runtime)
        {
            Console.WriteLine($"\nProcess Killing - {process.ProcessName} (PID: {process.Id})");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"   Status                 : Killing");
            Console.WriteLine($"   Runtime                : {runtime.TotalMinutes:F2} minutes");

            process.Kill();

            string log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                         $"{process.ProcessName} (PID: {process.Id}) was killed\n" +
                         $"Ran for {runtime.TotalMinutes:F2} minutes\n\n";
            Console.WriteLine(log);

            return log;
        }

        public static void LogToFile(string logs)
        {
            File.AppendAllText("logs.txt", logs);
        }
    }
}