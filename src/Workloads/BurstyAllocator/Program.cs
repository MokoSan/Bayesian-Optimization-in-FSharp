const long ITERATION_COUNT = 10_000_000;
const int REPEAT_COUNT = 10;

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
