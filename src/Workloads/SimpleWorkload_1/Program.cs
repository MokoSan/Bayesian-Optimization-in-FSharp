using CommandLine;

namespace SimpleWorkload_1
{
    class Program
    { 
        private const int DEFAULT_SLEEP_MSEC = 500;
        private const int FAST_SLEEP_MSEC    = 100;

        public class Options
        {
            [Option('i', "input", Required = true, HelpText = "An int that will correspond to a sleep time.")]
            public int Input { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                switch (o.Input)
                {
                    case int n when n >= 1.0 && n < 2:
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
