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

            public bool NoActiveProcesses(List<ProcessRR> pProcess)
            {
                foreach (ProcessRR process in pProcess)
                {
                    if (process.timeFinished == -1)
                    {
                        return false;
                    }
                }
                return true;
            }
            public int EnqueueNewActiveProcess(ref Queue<int> pProcesses, List<ProcessRR> processRRs, int currentState)
            {
                foreach (ProcessRR pProcess in processRRs)
                {
                    //Console.WriteLine($"{LetterForNumber(pProcess.ID)} || {currentState} || {pProcess._aTime}");
                    if (currentState == pProcess._aTime)
                    {
                        Console.WriteLine($"New process {LetterForNumber(pProcess.ID)} || {currentState}");
                        pProcesses.Enqueue(pProcess.ID);
                        return pProcess.ID;
                    }
                }
                return -1;
            }

            public void DisplayTableRR(int qSlice)
            {
                List<ProcessRR> activeProcesses = new List<ProcessRR>(Processes.Cast<ProcessRR>());
                int counter = 0;
                int Visits = 0;
                Queue<int> queue = new Queue<int>();
                do
                {
                    int NewProcess = EnqueueNewActiveProcess(ref queue, activeProcesses, counter);
                    int current = queue.Peek();
                    if (NewProcess == current)
                    {
                        counter++;
                        continue;
                    }
                    Visits++;
                    if (Visits == qSlice)
                    {
                        current = queue.Dequeue();
                        queue.Enqueue(current);
                        Visits = 0;
                    }
                    Console.WriteLine($"{LetterForNumber(current)} || {counter} || {Visits}");

                    activeProcesses[current].timeRemaining--;
                    if (activeProcesses[current].timeRemaining <= 0)
                    {
                        activeProcesses[current].timeFinished = counter;
                        List<int> temp = queue.ToList();
                        temp.Remove(current);
                        queue = new Queue<int>(temp);
                    }
                    counter++;
                }
                while (!NoActiveProcesses(activeProcesses));

                //
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

