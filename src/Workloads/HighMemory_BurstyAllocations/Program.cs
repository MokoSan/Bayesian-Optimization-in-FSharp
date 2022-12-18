using System.Diagnostics;
using System.Reflection;

using (Process highMemoryLoadProcess = new Process())
{
    highMemoryLoadProcess.StartInfo.FileName               = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "./make_memory_load.exe" );
    highMemoryLoadProcess.StartInfo.Arguments              = "-percent 90";
    highMemoryLoadProcess.StartInfo.UseShellExecute        = false;

    highMemoryLoadProcess.Start();

    Thread.Sleep(2000);

    const long ITERATION_COUNT = 10_000_000;
    const int REPEAT_COUNT     = 10;

    for (int repeatIdx = 0; repeatIdx < REPEAT_COUNT; repeatIdx++)
    {
        for (long iterationIdx = 0; iterationIdx < ITERATION_COUNT; iterationIdx++)
        {
            if (iterationIdx % 100 == 0)
            {
                int[] allocation0 = new int[10000];
                int[] allocation1 = new int[10000];
                int[] allocation2 = new int[10000];
                int[] allocation3 = new int[10000];
                int[] allocation4 = new int[10000];
                int[] allocation5 = new int[10000];
            }
        }
    }

    highMemoryLoadProcess.Kill();
}
