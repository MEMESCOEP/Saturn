/* DIRECTIVES */
using SDL2;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class Messagebox
    {
        /* FUNCTIONS */
        public static void ShowMessage(string Title, string Message, SDL.SDL_MessageBoxFlags MsgBoxFlags = SDL2.SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION)
        {
            SDL.SDL_ShowSimpleMessageBox(MsgBoxFlags, Title, Message, 0);
        }
    }
}