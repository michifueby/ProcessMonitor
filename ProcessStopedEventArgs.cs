using System;

namespace ProcessMonitor
{
    public class ProcessStopedEventArgs : EventArgs
    {
        public ProcessStopedEventArgs()
        {
            SafeProcess = new SafeProcess();
        }

        public SafeProcess SafeProcess
        {
            get;
            set;
        }
    }
}