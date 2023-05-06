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
                pProcess._id = _nextID;
                Processes.Add(pProcess);
                _nextID++;
            }

            public void RemoveAll()
            {
                Processes.Clear();
                _nextID = 0;
            }
            public bool NoActiveProcesses(List<Process> pProcess)
            {
                foreach (Process process in pProcess)
                {
                    if (process._finishTime == -1)
                    {
                        return false;
                    }
                }
                return true;
            }
            public int EnqueueNewActiveProcess(ref Queue<int> pProcesses, List<Process> pProcessRRs, int pCurrentState)
            {
                foreach (Process pProcess in pProcessRRs)
                {
                    //Console.WriteLine($"{LetterForNumber(pProcess.ID)} || {currentState} || {pProcess._aTime}");
                    if (pCurrentState == pProcess._arrivalTime)
                    {
                        //Console.WriteLine($"New process {LetterForNumber(pProcess.ID)} || {currentState}");
                        pProcesses.Enqueue(pProcess._id);
                        return pProcess._id;
                    }
                }
                return -1;
            }
            public void CreateDiagramRepresentation(out Dictionary<int,List<bool>> pDiagramRepresentation, List<Process> pProcessRRs)
            {
                pDiagramRepresentation = new Dictionary<int, List<bool>>();
                foreach (Process p in pProcessRRs)
                {
                    pDiagramRepresentation.Add(p._id, new List<bool>());
                }
            }
            public void AddToDiagramRepresentation(ref Dictionary<int,List<bool>> pDiagramRepresentation, int pId)
            {
                foreach(int Key in pDiagramRepresentation.Keys)
                {
                    if (Key == pId) pDiagramRepresentation[Key].Add(true);
                    else pDiagramRepresentation[Key].Add(false);
                }
            }
            public void DisplayGraphHeaders(ref Dictionary<int, List<bool>> pDiagramRepresentation)
            {
                string Output = "\n\nP |";
                for (int i = 1; i <= pDiagramRepresentation.Values.First().Count(); i++)
                {
                    Output += $"{i.ToString().PadLeft(2)}|";
                }
                Console.WriteLine(Output);
            }
            public void DisplayGraph(ref Dictionary<int, List<bool>> pDiagramRepresentation)
            {
                DisplayGraphHeaders(ref pDiagramRepresentation);
                foreach (int Header in pDiagramRepresentation.Keys)
                {
                    string Output = $"{LetterForNumber(Header)} |";
                    foreach(bool Value in pDiagramRepresentation[Header])
                    {
                        if (Value) Output += "[]|";
                        else Output += "  |";
                    }
                    Console.WriteLine(Output);
                }
            }

            public void DisplayTableRR(int qSlice)
            {
                List<Process> activeProcesses = new List<Process>(Processes.Cast<Process>());
                CreateDiagramRepresentation(out Dictionary<int, List<bool>> diagramRepresentation, activeProcesses);
                int counter = 0;
                int visits = 0;
                Queue<int> queue = new Queue<int>();
                do
                {
                    int newProcess = EnqueueNewActiveProcess(ref queue, activeProcesses, counter);
                    int current = queue.Peek();
                    if (newProcess == current)
                    {
                        counter++;
                        continue;
                    }
                    visits++;
                    AddToDiagramRepresentation(ref diagramRepresentation, current);
                    if (visits == qSlice)
                    {
                        current = queue.Dequeue();
                        queue.Enqueue(current);
                        visits = 0;
                    }
                    //Console.WriteLine($"{LetterForNumber(current)} || {counter} || {Visits}");

                    activeProcesses[current]._timeRemaining--;
                    if (activeProcesses[current]._timeRemaining <= 0)
                    {
                        activeProcesses[current]._finishTime = counter;
                        List<int> temp = queue.ToList();
                        temp.Remove(current);
                        queue = new Queue<int>(temp);
                        visits = 0;
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
                foreach (Process pProcess in Processes)
                {
                    int finishTime = pProcess._finishTime;
                    int turnaroundTime = finishTime - pProcess._arrivalTime;
                    double normalisedTurnaroundTime = (double)turnaroundTime / (double)pProcess._startTime;
                    meanTurnaroundTime += turnaroundTime;
                    meanNormalisedTurnaroundTime += normalisedTurnaroundTime;
                    Console.WriteLine("{0,3} | {1,3} | {2,3} | {3,3} | {4,6} | {5,6:N3}", LetterForNumber(pProcess._id), pProcess._arrivalTime, pProcess._startTime, finishTime, turnaroundTime, normalisedTurnaroundTime);
                    currentStart = finishTime;
                }
                meanTurnaroundTime = meanTurnaroundTime / Processes.Count;
                meanNormalisedTurnaroundTime = meanNormalisedTurnaroundTime / Processes.Count;
                Console.WriteLine("Mean|     |     |     | {0,6:N3} | {1,6:N3}", meanTurnaroundTime, meanNormalisedTurnaroundTime);
                DisplayGraph(ref diagramRepresentation);
            }

            public string LetterForNumber(int pNum)
            {
                string strAlpha = ((char)(pNum + 65)).ToString();
                return strAlpha; 
            }
        }

        public class Process
        {
            public int _id;
            public int _arrivalTime { get; private set; }
            public int _startTime { get; private set; }
            public int _timeRemaining;
            public int _finishTime = -1;

            public Process (int pArrivalTime, int pStartTime)
            {
                _arrivalTime = pArrivalTime;
                _startTime = pStartTime;
                _timeRemaining = pStartTime;
            }
        }
    }
}

