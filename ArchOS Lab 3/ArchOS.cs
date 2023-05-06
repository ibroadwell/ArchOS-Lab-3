using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ArchOS
{
    public class MethodsForLab3
    {
        public class ProcessManager
        {
            public List<Process> Processes { get; private set; }

            public ProcessManager()
            {
                Processes = new List<Process>();
            }


            private static int _nextID = 0;

            public int nextID
            {
                get { return _nextID; }
            }
            public void AddProcess(Process pProcess)
            {
                pProcess.ID = _nextID;
                Processes.Add(pProcess);
                _nextID++;
            }

            public void RemoveAll()
            {
                Processes.Clear();
                _nextID = 0;
            }

            public void DisplayTable()
            {
                Console.WriteLine("{0,3} | {1,3} | {2,3}", "P", "Ta", "Ts");
                foreach (Process pProcess in Processes) 
                {
                    Console.WriteLine("{0,3} | {1,3} | {2,3}", LetterForNumber(pProcess.ID), pProcess._aTime, pProcess._sTime);
                }
            }

            public void DisplayFullTable()
            {
                Console.WriteLine("{0,3} | {1,3} | {2,3} | {3,3} | {4,6} | {5,6}", "P", "Ta", "Ts", "Tf", "Tt", "Ttn");
                int currentStart = 0;
                double meanTurnaroundTime = 0;
                double meanNormalisedTurnaroundTime = 0;
                foreach (Process pProcess in Processes)
                {
                    int finishTime = (currentStart + pProcess._sTime);
                    int turnaroundTime = finishTime - pProcess._aTime;
                    double normalisedTurnaroundTime = (double)turnaroundTime / (double)pProcess._sTime;
                    meanTurnaroundTime += turnaroundTime;
                    meanNormalisedTurnaroundTime += normalisedTurnaroundTime;
                    Console.WriteLine("{0,3} | {1,3} | {2,3} | {3,3} | {4,6} | {5,6:N3}", LetterForNumber(pProcess.ID), pProcess._aTime, pProcess._sTime, finishTime, turnaroundTime, normalisedTurnaroundTime);
                    currentStart = finishTime;
                }
                meanTurnaroundTime = meanTurnaroundTime / Processes.Count;
                meanNormalisedTurnaroundTime = meanNormalisedTurnaroundTime / Processes.Count;
                Console.WriteLine("Mean|     |     |     | {0,6:N3} | {1,6:N3}", meanTurnaroundTime, meanNormalisedTurnaroundTime);
            }

            public void DisplayGanttChart()
            {
                int counter = 0;
                Console.WriteLine("P |__|__|__|__|__5__|__|__|__|__10_|__|__|__|__15_|__|__|__|__20_|__|__|__|__25_|__|__|__|__30_|__|__|__|__35");
                int currentStart = 0;
                foreach (Process pProcess in Processes)
                {
                    int finishTime = (currentStart + pProcess._sTime);
                    Console.Write(LetterForNumber(pProcess.ID) + " |");
                    if (currentStart != 0)
                    {
                        for (int i = 0; i < pProcess._aTime; i++)
                        {
                            Console.Write("  |");
                        }
                    }
                    
                    if (pProcess._aTime != currentStart)
                    {
                        for (int i = pProcess._aTime; i < currentStart; i++)
                        {
                            Console.Write("--|");
                        }
                    }
                    for (int i = currentStart; i < finishTime; i++)
                    {
                        Console.Write("[]|");
                    }
                    for (int i = finishTime; i < 35; i++)
                    {
                        Console.Write("  |");
                    }
                    currentStart = finishTime;
                    
                    Console.WriteLine();
                }
            }

            public ProcessRR[] ActiveProcesses(int currentState)
            {
                List<ProcessRR> result = new List<ProcessRR>();
                foreach (ProcessRR pProcess in Processes)
                {
                    if (currentState >= pProcess._aTime && pProcess.timeFinished == -1)
                    {
                        result.Add(pProcess);
                    }
                    else
                    {
                        result.Add(null);
                    }
                    
                }
                return result.ToArray();
            }

            public int ActiveProcessesLength(ProcessRR[] pProcess)
            {
                int count = 0;
                foreach (ProcessRR p in pProcess)
                {
                    if (p != null)
                    {
                        count++;
                    }

                }
                return count;
            }

            public bool NoActiveProcesses(ProcessRR[] pProcess)
            {
                foreach (ProcessRR process in pProcess)
                {
                    if (process != null)
                    {
                        return false;
                    }
                }
                return true;
            }
            public void DisplayTableRR(int qSlice)
            {
                int counter = 0;
                ProcessRR[] activeProcesses = ActiveProcesses(counter);
                Queue<int> queue = new Queue<int>();
                for (int i = 0; i < activeProcesses.Length; i++)
                {
                    if (activeProcesses[i] != null)
                    {
                        queue.Enqueue(activeProcesses[i].ID);
                    }
                }
                do
                {
                    for (int p = queue.Count - 1; p >= 0; p--)
                    {
                        ProcessRR process = activeProcesses[queue.First()];
                        if (process == null)
                        { 
                            continue;
                        }
                        for (int i = 0; i < qSlice && i < activeProcesses[queue.First()].timeRemaining; i++)
                        {
                            activeProcesses[queue.First()].timeRemaining--;
                            counter++;
                            Console.Write( LetterForNumber(activeProcesses[queue.First()].ID));
                            if (activeProcesses[queue.First()].timeRemaining == 0)
                            {
                                activeProcesses[queue.First()].timeFinished = counter;

                                
                            }
                            activeProcesses = ActiveProcesses(counter);
                        }

                    }    
                }
                while (!NoActiveProcesses(activeProcesses));
                Console.WriteLine();
                Console.WriteLine("{0,3} | {1,3} | {2,3} | {3,3} | {4,6} | {5,6}", "P", "Ta", "Ts", "Tf", "Tt", "Ttn");
                int currentStart = 0;
                double meanTurnaroundTime = 0;
                double meanNormalisedTurnaroundTime = 0;
                foreach (ProcessRR pProcess in Processes)
                {
                    int finishTime = pProcess.timeFinished;
                    int turnaroundTime = finishTime - pProcess._aTime;
                    double normalisedTurnaroundTime = (double)turnaroundTime / (double)pProcess._sTime;
                    meanTurnaroundTime += turnaroundTime;
                    meanNormalisedTurnaroundTime += normalisedTurnaroundTime;
                    Console.WriteLine("{0,3} | {1,3} | {2,3} | {3,3} | {4,6} | {5,6:N3}", LetterForNumber(pProcess.ID), pProcess._aTime, pProcess._sTime, finishTime, turnaroundTime, normalisedTurnaroundTime);
                    currentStart = finishTime;
                }
                meanTurnaroundTime = meanTurnaroundTime / Processes.Count;
                meanNormalisedTurnaroundTime = meanNormalisedTurnaroundTime / Processes.Count;
                Console.WriteLine("Mean|     |     |     | {0,6:N3} | {1,6:N3}", meanTurnaroundTime, meanNormalisedTurnaroundTime);
            }

            public string LetterForNumber(int pNum)

            {
                string strAlpha = ((char)(pNum + 65)).ToString();
                return strAlpha; 
            }
        }
        public class Process
        {
            public int ID;
            public int _aTime;
            public int _sTime;

            public Process(int aTime, int sTime)
            {
                _aTime = aTime;
                _sTime = sTime;
            }

        }

        public class ProcessRR : Process
        {
            public int timeRemaining;
            public int timeFinished = -1;

            public ProcessRR (int aTime, int sTime) : base (aTime, sTime)
            {
                timeRemaining = sTime;
            }
        }
    }
}

