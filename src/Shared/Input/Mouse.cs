/* DIRECTIVES */
using System.Numerics;
using Raylib_cs;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class Mouse
    {
        /* VARIABLES */        
        public static Vector2 MouseClickPosition = new Vector2(0, 0);
        public static Vector2 LastMousePosition = new Vector2(0, 0);
        public static Vector2 MousePosition = new Vector2(0, 0);
        public static Vector2 MouseDelta = new Vector2(0, 0);
        public static float WheelDelta = 0f;
        public static bool MouseMoved = true;


        /* FUNCTIONS */
        public static void UpdateMouseProperties()
        {
            LastMousePosition = MousePosition;
            MousePosition = Raylib.GetMousePosition();
            MouseDelta = Raylib.GetMouseDelta();
            MouseMoved = MouseDelta != Vector2.Zero;

            if (Raylib.IsMouseButtonPressed(MouseButton.Left) || Raylib.IsMouseButtonPressed(MouseButton.Right) || Raylib.IsMouseButtonPressed(MouseButton.Middle))
            {
                MouseClickPosition = MousePosition;
            }

            WheelDelta = Raylib.GetMouseWheelMove();
            
            /*if (Raylib.IsMouseButtonReleased(MouseButton.Left) || Raylib.IsMouseButtonReleased(MouseButton.Right))
            {
                MouseClickPosition = Vector2.Zero;
            }*/
        }
    }
}