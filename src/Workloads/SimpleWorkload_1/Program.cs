using CommandLine;

// A "smoke-test" of a workload that sleeps for the least amount of time if the input to the program is in the range [1., 2).
namespace SimpleWorkload_1
{
    class Program
    { 
        private const int DEFAULT_SLEEP_MSEC = 500;
        private const int FAST_SLEEP_MSEC    = 100;

        public class Options
        {
            [Option('i', "input", Required = true, HelpText = "An int that will correspond to a sleep time.")]
            public double Input { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                switch (o.Input)
                {
                    case double n when n >= 1.0 && n < 2.0:
                        {
                            Thread.Sleep(FAST_SLEEP_MSEC);
                            break;
                        }

                    default:
                        {
                            Thread.Sleep(DEFAULT_SLEEP_MSEC);
                            break;
                        }
                }
            });
        }
    }
}
