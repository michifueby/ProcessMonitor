namespace ProcessMonitor
{
    using System;

    public class Application
    {
        public void Run()
        {
            ProcessMonitor processMonitor = new ProcessMonitor();

            processMonitor.PollInterval = TimeSpan.FromSeconds(1);
            processMonitor.ProcessStarted += (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("New process started:");
                Console.WriteLine($"      Name: {e.SafeProcess.ProcessName}");
                Console.WriteLine($"Executable: {e.SafeProcess.MainModule.Filename}");
                Console.WriteLine($"        Id: {e.SafeProcess.Id}");
                Console.WriteLine($" StartTime: {e.SafeProcess.StartTime}\n");
                Console.ResetColor();
            };
            processMonitor.ProcessStoped += (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Process stopped:");
                Console.WriteLine($"      Name: {e.SafeProcess.ProcessName}");
                Console.WriteLine($"Executable: {e.SafeProcess.MainModule.Filename}");
                Console.WriteLine($"        Id: {e.SafeProcess.Id}");
                Console.WriteLine($" StartTime: {e.SafeProcess.StartTime}");
                Console.WriteLine($"  ExitCode: {e.SafeProcess.ExitCode}\n");
                Console.ResetColor();
            };

            processMonitor.Start();
        }
    }
}
