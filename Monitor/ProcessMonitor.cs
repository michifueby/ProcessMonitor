namespace ProcessMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    public class ProcessMonitor
    {
        public event EventHandler<ProcessStartedEventArgs> ProcessStarted;

        public event EventHandler<ProcessStopedEventArgs> ProcessStoped;

        private Thread proccessThread;

        private ProcessThreadArguments processThreadArguments;

        private ProcessHandleThreadArguments processHandleThreadArguments;

        private Thread processHandleThread;

        private List<Process> processList;

        public ProcessMonitor()
        {
            processThreadArguments = new ProcessThreadArguments();

            processHandleThreadArguments = new ProcessHandleThreadArguments();

            processList = new List<Process>();
        }

        public TimeSpan PollInterval
        {
            get;
            set;
        }

        public void Start()
        {
            if (this.proccessThread != null && this.proccessThread.IsAlive)
            {
                throw new InvalidOperationException("The process thread is already running!");
            }

            this.processThreadArguments.Exit = false;
            this.proccessThread = new Thread(Worker);
            this.proccessThread.Start(this.processThreadArguments);
            StartProcessHandle();
        }

        public void StartProcessHandle()
        {
            if (this.processHandleThread != null && this.processHandleThread.IsAlive)
            {
                throw new InvalidOperationException("The process handle thread is already running!");
            }

            this.processHandleThreadArguments.Exit = false;
            this.processHandleThread = new Thread(ProcessHandleWorker);
            this.processHandleThread.Start(this.processHandleThreadArguments);
        }

        public void StopProcessHandle()
        {
            if (this.processHandleThread == null || this.processHandleThread.IsAlive)
            {
                throw new InvalidOperationException("The process handle thread is already running!");
            }

            this.processHandleThreadArguments.Exit = true;
        }

        public void Stop()
        {
            if (this.proccessThread == null || !this.proccessThread.IsAlive)
            {
                throw new InvalidOperationException("The process handle thread is already stopped!");
            }

            this.processThreadArguments.Exit = true;

            this.StopProcessHandle();
        }

        private void Worker(object data)
        {
            ProcessThreadArguments args = (ProcessThreadArguments)data;

            while (!args.Exit)
            {
                Process[] allProcessesStarted = Process.GetProcesses();

                this.CompareProcesses(allProcessesStarted);

                Thread.Sleep(PollInterval);
            }
        }

        private void ProcessHandleWorker(object data)
        {
            ProcessHandleThreadArguments args = (ProcessHandleThreadArguments)data;
            
            while (!args.Exit)
            {
                Console.SetCursorPosition(0, Console.CursorTop);

                string input = Console.ReadLine();

                this.ProcessCommandInput(input);
            }
        }

        private void CompareProcesses(Process[] allProcessesStarted)
        {
            ProcessStartedEventArgs processStartedEventArgs = new ProcessStartedEventArgs();

            ProcessStopedEventArgs processStopedEventArgs = new ProcessStopedEventArgs();

            if (processList.Count != 0)
            {
                var stopedProcesses = processList.Where(p1 => allProcessesStarted.All(p2 => p2.Id != p1.Id)).ToList();

                for (int i = 0; i < stopedProcesses.Count; i++)
                {
                    try
                    {
                        processStopedEventArgs.SafeProcess.ProcessName = stopedProcesses[i].ProcessName;
                        processStopedEventArgs.SafeProcess.Id = stopedProcesses[i].Id;
                        processStopedEventArgs.SafeProcess.StartTime = stopedProcesses[i].StartTime;
                        processStopedEventArgs.SafeProcess.ExitCode = stopedProcesses[i].ExitCode;
                        processStopedEventArgs.SafeProcess.MainModule.Filename = stopedProcesses[i].MainModule.FileName;
                    }
                    catch (Exception)
                    {
                        Console.Write("");
                    }

                    this.FireOnProcessStoped(processStopedEventArgs);

                    this.processList.RemoveAll(x => x.Id == stopedProcesses[i].Id);
                    stopedProcesses.RemoveAt(i);
                }

                var newProcessStartedList = allProcessesStarted.Where(p1 => processList.All(p2 => p2.Id != p1.Id)).ToList();

                if (newProcessStartedList.Count != 0)
                {
                    for (int i = 0; i < newProcessStartedList.Count; i++)
                    {
                        processList.Add(newProcessStartedList[i]);

                        try
                        {
                            processStartedEventArgs.SafeProcess.ProcessName = newProcessStartedList[i].ProcessName;
                            processStartedEventArgs.SafeProcess.Id = newProcessStartedList[i].Id;
                            processStartedEventArgs.SafeProcess.StartTime = newProcessStartedList[i].StartTime;
                            processStartedEventArgs.SafeProcess.MainModule.Filename = stopedProcesses[i].MainModule.FileName;
                        }
                        catch
                        {
                            Console.Write("");
                        }

                        FireOnProcessStarted(processStartedEventArgs);
                        newProcessStartedList.RemoveAt(i);
                    }
                }
            }
            else
            {
                foreach (var process in allProcessesStarted)
                {
                    processList.Add(process);

                    try
                    {
                        processStartedEventArgs.SafeProcess.ProcessName = process.ProcessName;
                        processStartedEventArgs.SafeProcess.Id = process.Id;
                        processStartedEventArgs.SafeProcess.StartTime = process.StartTime;
                        processStartedEventArgs.SafeProcess.MainModule.Filename = process.MainModule.FileName;
                    }
                    catch (Exception)
                    {
                        Console.Write("");
                    }

                    FireOnProcessStarted(processStartedEventArgs);
                }
            }
        }

        protected virtual void FireOnProcessStarted(ProcessStartedEventArgs e)
        {
            this.ProcessStarted?.Invoke(this, e);
        }

        protected virtual void FireOnProcessStoped(ProcessStopedEventArgs e)
        {
            this.ProcessStoped?.Invoke(this, e);
        }

        private void ProcessCommandInput(string input)
        {
            if (input.Contains("start"))
            {
                input = input.Remove(0, 5);
                StartProcess(input);
            }
            else if (input.Contains("stop"))
            {
                input = input.Remove(0, 4);

                if (!StopProcess(input))
                {
                    Console.WriteLine("Not valid pid!");
                }
            }
            else
            {
                Console.WriteLine("Not valid Command!");
            }
        }

        private void StartProcess(string input)
        {
            Process process = new Process();

            string[] command = input.Split();

            try
            {
                for (int i = 1; i < command.Length; i++)
                {
                    process.StartInfo.FileName = command[i];

                    if (i == 2)
                    {
                        process.StartInfo.Arguments = command[i];
                    }
                }

                process.Start();
            }
            catch (Exception)
            {
                Console.WriteLine("Error while starting the process!");
            }
        }

        private bool StopProcess(string input)
        {
            int number = 0;
            bool isInputValid;

            isInputValid = int.TryParse(input, out number);

            try
            {
                Process.GetProcessById(number).Kill();
            }
            catch (Exception)
            {
                Console.WriteLine("Error while kill the process");
            }

            return isInputValid;
        }
    }
}

