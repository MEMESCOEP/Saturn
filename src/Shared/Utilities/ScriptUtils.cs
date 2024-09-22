/* DIRECTIVES */
using System.Numerics;
using Hexa.NET.Raylib;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class ScriptUtils
    {
        /* VARIABLES */
        public static Vector2 CurrentWindowSize = Vector2.One * 100;


        /* CLASSES */
        public class WindowUtils
        {
            /* FUNCTIONS */
            public static void ResizeGameWindow(int Width, int Height)
            {
                ConsoleUtils.StatusWrite($"Setting window size to {Width}x{Height}...");
                Raylib.SetWindowMinSize(Width, Height);
                Raylib.SetWindowSize(Width, Height);
            }

            public static void SetWindowConfig(params object[] WindowFlags)
            {
                try
                {
                    // Make sure each ConfigFlag has a boolean paired with it
                    if (WindowFlags.Count() % 2 != 0)
                    {
                        throw new Exception("Invalid number of values for window flags!");
                    }

                    // Parse each ConfigFlag
                    for (int FlagIndex = 0; FlagIndex < WindowFlags.Count(); FlagIndex += 2)
                    {
                        ConfigFlags Flag = (ConfigFlags)WindowFlags[FlagIndex];

                        if ((bool)WindowFlags[FlagIndex + 1] == true)
                        {
                            ConsoleUtils.StatusWrite($"Setting flag \"{Flag}\" (Flag value: \"0x{Flag.ToString("x")}\")...");
                            Raylib.SetWindowState((uint)Flag);
                        }
                        else
                        {
                            ConsoleUtils.StatusWrite($"Clearing flag \"{Flag}\" (Flag value: \"0x{Flag.ToString("x")}\")...");
                            Raylib.ClearWindowState((uint)Flag);
                        }
                    }
                }
                catch (Exception EX)
                {
                    ConsoleUtils.StatusWrite($"Failed to set window flags: {EX.Message}", ConsoleUtils.StatusTypes.ERROR);
                }
            }

            public static bool GetFlagState(ConfigFlags WindowFlag)
            {
                return Raylib.IsWindowState((uint)WindowFlag);
            }

            public static bool IsCursorHidden()
            {
                return Raylib.IsCursorHidden();
            }

            public static void SetCursorState(bool Hidden, bool Locked)
            {
                ConsoleUtils.StatusWrite($"Setting cursor state (Hidden = {Hidden}, Locked = {Locked})...");
                
                if (Hidden == true)
                {
                    Raylib.HideCursor();
                }
                else
                {
                    Raylib.ShowCursor();
                }

                if (Locked == true)
                {
                    Raylib.DisableCursor();
                }
                else
                {
                    Raylib.EnableCursor();
                }
            }

            public static void SetWindowName(string Title)
            {
                ConsoleUtils.StatusWrite($"Setting window title to \"{Title}\"...");
                Raylib.SetWindowTitle(Title);
            }
        }

        public class EngineUtils
        {
            /* FUNCTIONS */
            public static void CloseEngine(int ExitCode)
            {
                EntryPoint.ExitEngine(ExitCode);
            }
        }

        /* FUNCTIONS */
        public static unsafe IntPtr CreatePointer(object Object)
        {
            var NewPointer = &Object;
            return (IntPtr)NewPointer;
        }
    }
}