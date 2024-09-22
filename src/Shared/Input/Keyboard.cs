/* DIRECTIVES */
using Hexa.NET.Raylib;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class Keyboard
    {
        /* FUNCTIONS */
        // The following are some raylib wrapper functions. These exist because NLua wants to see two functions with the exact same type as different smh
        public static bool IsKeyPressed(KeyboardKey Key)
        {
            return Raylib.IsKeyPressed((int)Key);
        }

        public static bool IsKeyDown(KeyboardKey Key)
        {
            return Raylib.IsKeyDown((int)Key);
        }

        public static bool IsKeyReleased(KeyboardKey Key)
        {
            return Raylib.IsKeyReleased((int)Key);
        }

        public static bool IsKeyUp(KeyboardKey Key)
        {
            return Raylib.IsKeyUp((int)Key);
        }

        public static void SetExitKey(KeyboardKey Key)
        {
            ConsoleUtils.StatusWrite($"Setting exit key to \"{Key}\"...");
            Raylib.SetExitKey((int)Key);
        }
    }
}