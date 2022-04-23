namespace ProcessMonitor
{
    using System;

    public class SafeProcess
    {
        public SafeProcess()
        {
            MainModule = new MainModule();
        }

        public string ProcessName
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public DateTime StartTime
        {
            get;
            set;
        }

        public DateTime ExitTime
        {
            get;
            set;
        }

        public int ExitCode
        {
            get;
            set;
        }

        public MainModule MainModule
        {
            get;
            set;
        }
    }
}
