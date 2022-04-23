namespace ProcessMonitor
{
    using System;

    public class ProcessStartedEventArgs : EventArgs
    {
       public ProcessStartedEventArgs()
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