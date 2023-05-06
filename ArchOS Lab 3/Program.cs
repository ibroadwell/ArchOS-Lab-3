// See https://aka.ms/new-console-template for more information
using static ArchOS.MethodsForLab3;

ProcessManager manager = new ProcessManager();
int[,] intProcesses = { { 0, 3 }, { 2, 6 }, { 5, 5 }, { 6, 3 }, { 8, 6 }, { 9, 2 }, { 10, 6 } };
DisplayValues(intProcesses, 0);
DisplayValues(intProcesses, 1);
DisplayValues(intProcesses, 3);
DisplayValues(intProcesses, 4);
DisplayValues(intProcesses, 6);


void DisplayValues(int[,] values, int slice)
{
    manager.RemoveAll();
    for (int i = 0; i < values.GetLength(0); i++)
    {
        Process process = new Process(values[i, 0], values[i, 1]);
        manager.AddProcess(process);
    }
    if (slice <= 0) manager.DisplayTableRR(1000000);
    else manager.DisplayTableRR(slice);
}



