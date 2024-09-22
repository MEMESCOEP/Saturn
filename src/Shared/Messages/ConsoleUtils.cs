/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class ConsoleUtils
    {
        /* ENUMS */
        public enum StatusTypes
        {
            INFO,
            ERROR,
            WARNING,
            DEBUG
        }


        /* VARIABLES */
        public static string BOLD        = Console.IsOutputRedirected ? "" : "\x1b[1m";
        public static string NOBOLD      = Console.IsOutputRedirected ? "" : "\x1b[22m";
        public static string UNDERLINE   = Console.IsOutputRedirected ? "" : "\x1b[4m";
        public static string NOUNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[24m";
        public static string REVERSE     = Console.IsOutputRedirected ? "" : "\x1b[7m";
        public static string NOREVERSE   = Console.IsOutputRedirected ? "" : "\x1b[27m";


        /* FUNCTIONS */
        public static void StatusWrite(string Message, StatusTypes StatusType = StatusTypes.INFO)
        {
            switch (StatusType)
            {
                case StatusTypes.INFO:
                    Console.WriteLine($"[{ColorUtils.GREEN}INFO{ColorUtils.NORMAL}] >> {Message}");
                    break;

                case StatusTypes.ERROR:
                    Console.WriteLine($"[{ColorUtils.RED}{BOLD}ERROR{NOBOLD}{ColorUtils.NORMAL}] >> {Message}");
                    break;

                case StatusTypes.WARNING:
                    Console.WriteLine($"[{ColorUtils.YELLOW}WARN{ColorUtils.NORMAL}] >> {Message}");
                    break;

                case StatusTypes.DEBUG:
                    if (EntryPoint.Debug == true)
                    {
                        Console.WriteLine($"[{ColorUtils.MAGENTA}DEBUG{ColorUtils.NORMAL}] >> {Message}");
                    }
                    
                    break;
            }
        }
    }
}