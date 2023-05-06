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
        /// <summary>
        /// The manager of the operation objects.
        /// </summary>
        public class ProcessManager
        {
            public List<Process> Processes { get; private set; } // List of all processes
            public ProcessManager() { Processes = new List<Process>(); }
            private static int _nextID = 0; // Identifier for the next id of the process

            /// <summary>
            /// Adds a process to the manager.
            /// </summary>
            /// <param name="pProcess">An operation</param>
            public void AddProcess(Process pProcess)
            {
                pProcess._id = _nextID;
                Processes.Add(pProcess);
                _nextID++;
            }

            /// <summary>
            /// Used to clear the manager of processes.
            /// </summary>
            public void RemoveAll()
            {
                Processes.Clear();
                _nextID = 0;
            }

            /// <summary>
            /// Used to workout if there are any unfinished processes.
            /// </summary>
            /// <returns>True: No processes remain to finish.</returns>
            public bool NoActiveProcesses()
            {
                foreach (Process process in Processes)
                {
                    if (process._finishTime == -1)
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Used to add a process to the queue if its arrival time has been met.
            /// </summary>
            /// <param name="pProcesses">Queue of processes being worked through</param>
            /// <param name="pCurrentState">Current stage</param>
            /// <returns>Id of the new process</returns>
            public int EnqueueNewActiveProcess(ref Queue<int> pProcesses, int pCurrentState)
            {
                foreach (Process pProcess in Processes)
                {
                    if (pCurrentState == pProcess._arrivalTime)
                    {
                        pProcesses.Enqueue(pProcess._id);
                        return pProcess._id;
                    }
                }
                return -1;
            }

            #region Diagram
            /// <summary>
            /// Creates the initial Table-Row system for the processes.
            /// </summary>
            /// <param name="pDiagramRepresentation"> Table-Dictionary where Keys = Headers and Value = Row </param>
            public void CreateDiagramRepresentation(out Dictionary<int,List<bool>> pDiagramRepresentation)
            {
                pDiagramRepresentation = new Dictionary<int, List<bool>>();
                foreach (Process p in Processes)
                {
                    pDiagramRepresentation.Add(p._id, new List<bool>());
                }
            }

            /// <summary>
            /// Fills in the information of the Table-Row system.
            /// </summary>
            /// <param name="pDiagramRepresentation"> Table-Dictionary where Keys = Headers and Value = Row </param>
            public void AddToDiagramRepresentation(ref Dictionary<int,List<bool>> pDiagramRepresentation, int pId)
            {
                foreach(int Key in pDiagramRepresentation.Keys)
                {
                    if (Key == pId) pDiagramRepresentation[Key].Add(true);
                    else pDiagramRepresentation[Key].Add(false);
                }
            }

            /// <summary>
            /// Displays the headers for the Table-Row system.
            /// </summary>
            /// <param name="pDiagramRepresentation"> Table-Dictionary where Keys = Headers and Value = Row </param>
            public void DisplayGraphHeaders(ref Dictionary<int, List<bool>> pDiagramRepresentation)
            {
                string Output = "\n\nP |";
                for (int i = 1; i <= pDiagramRepresentation.Values.First().Count(); i++)
                {
                    Output += $"{i.ToString().PadLeft(2)}|";
                }
                Console.WriteLine(Output);
            }

            /// <summary>
            /// Displays a Gantt graph of the Table-Row system.
            /// </summary>
            /// <param name="pDiagramRepresentation"> Table-Dictionary where Keys = Headers and Value = Row </param>
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
            #endregion

            /// <summary>
            /// Creates the detail table for each process and the Gantt graph of the time slices.
            /// </summary>
            /// <param name="qSlice">Allocated slice of time per process</param>
            public void DisplayTableRR(int qSlice)
            {
                CreateDiagramRepresentation(out Dictionary<int, List<bool>> diagramRepresentation);
                int counter = 0;
                int visits = 0;
                Queue<int> queue = new Queue<int>();
                do
                {
                    int newProcess = EnqueueNewActiveProcess(ref queue, counter);
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
                    Processes[current]._timeRemaining--;
                    if (Processes[current]._timeRemaining <= 0)
                    {
                        Processes[current]._finishTime = counter;
                        List<int> temp = queue.ToList();
                        temp.Remove(current);
                        queue = new Queue<int>(temp);
                        visits = 0;
                    }
                    counter++;
                }
                while (!NoActiveProcesses());
                Console.WriteLine("\n{0,3} | {1,3} | {2,3} | {3,3} | {4,6} | {5,6}", "P", "Ta", "Ts", "Tf", "Tt", "Ttn");
                double meanTurnaroundTime = 0;
                double meanNormalisedTurnaroundTime = 0;
                foreach (Process pProcess in Processes)
                {
                    int finishTime = pProcess._finishTime;
                    int turnaroundTime = finishTime - pProcess._arrivalTime;
                    double normalisedTurnaroundTime = (double)turnaroundTime / (double)pProcess._startTime;
                    meanTurnaroundTime += turnaroundTime;
                    meanNormalisedTurnaroundTime += normalisedTurnaroundTime;
                    Console.WriteLine("{0,3} | {1,3} | {2,3} | {3,3} | {4,6} | {5,6:N3}", LetterForNumber(pProcess._id), 
                        pProcess._arrivalTime, pProcess._startTime, finishTime, turnaroundTime, normalisedTurnaroundTime);
                }
                meanTurnaroundTime = meanTurnaroundTime / Processes.Count;
                meanNormalisedTurnaroundTime = meanNormalisedTurnaroundTime / Processes.Count;
                Console.WriteLine("Mean|     |     |     | {0,6:N3} | {1,6:N3}", meanTurnaroundTime, meanNormalisedTurnaroundTime);
                DisplayGraph(ref diagramRepresentation);
            }

            /// <summary>
            /// Converts the numbers into Alphabetical Characters.
            /// </summary>
            /// <param name="pNum">Number to convert</param>
            /// <returns>Number as its Alphabetical counterpart</returns>
            public string LetterForNumber(int pNum)
            {
                string strAlpha = ((char)(pNum + 65)).ToString();
                return strAlpha; 
            }
        }

        /// <summary>
        /// The object used to represent each operation.
        /// </summary>
        public class Process
        {
            public int _id; // Unique identifier for the process
            public int _arrivalTime { get; private set; } // When the process is supposed to arrive
            public int _startTime { get; private set; } // When the process started
            public int _timeRemaining;
            public int _finishTime = -1; // When the process finished

            public Process (int pArrivalTime, int pStartTime)
            {
                _arrivalTime = pArrivalTime;
                _startTime = pStartTime;
                _timeRemaining = pStartTime;
            }
        }
    }
}

