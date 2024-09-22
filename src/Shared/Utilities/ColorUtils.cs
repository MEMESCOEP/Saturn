/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class ColorUtils
    {
        /* VARIABLES */
        public static string NORMAL      = Console.IsOutputRedirected ? "" : "\x1b[39m";
        public static string RED         = Console.IsOutputRedirected ? "" : "\x1b[91m";
        public static string GREEN       = Console.IsOutputRedirected ? "" : "\x1b[92m";
        public static string YELLOW      = Console.IsOutputRedirected ? "" : "\x1b[93m";
        public static string BLUE        = Console.IsOutputRedirected ? "" : "\x1b[94m";
        public static string MAGENTA     = Console.IsOutputRedirected ? "" : "\x1b[95m";
        public static string CYAN        = Console.IsOutputRedirected ? "" : "\x1b[96m";
        public static string GREY        = Console.IsOutputRedirected ? "" : "\x1b[97m";
    }
}