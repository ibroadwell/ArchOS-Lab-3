// See https://aka.ms/new-console-template for more information
using ArchOS;
using static ArchOS.MethodsForLab3;

ProcessManager manager = new ProcessManager();
int[,] intProcesses = { { 0, 3 }, { 2, 6 }, { 5, 5 }, { 6, 3 }, { 8, 6 }, { 9, 2 }, { 10, 6 } };
for (int i = 0; i < intProcesses.GetLength(0); i++)
{
    Process process = new Process(intProcesses[i, 0], intProcesses[i, 1]);
    manager.AddProcess(process);
}
manager.DisplayTable();
Console.WriteLine();
manager.DisplayFullTable();
Console.WriteLine();
manager.DisplayGanttChart();

manager.RemoveAll();

int[,] intProcesses2 = { { 0, 3 }, { 2, 6 }, { 4, 4 }, { 6, 5 }, { 8, 2 } };

for (int i = 0; i < intProcesses2.GetLength(0); i++)
{
    ProcessRR process = new ProcessRR(intProcesses2[i, 0], intProcesses2[i, 1]);
    manager.AddProcess(process);
}
manager.DisplayTableRR(4);

