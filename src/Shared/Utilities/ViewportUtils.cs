/* DIRECTIVES */
using System.Numerics;
using Hexa.NET.Raylib;
using Color = Hexa.NET.Raylib.Color;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class ViewportUtils
    {
        /* VARIABLES */
        public static Camera2D Camera = new Camera2D();
        public static Vector3 SelectedObjOutlineGrowth = Vector3.One * 0.1f;
        public static Vector3 CameraRight = Vector3.Zero;
        public static Vector3 CameraUp = Vector3.Zero;
        public static Vector2 AddNewObjectWinPos = Vector2.Zero;
        public static Vector2 MouseSensitivity = new Vector2(0.02f, 0.02f);
        public static Color SelectedObjOutlineColor = Raylib.Red;
        public static Color GridColor = new Color(32, 32, 32, 255);
        public static float GridSpacing = 8f;
        public static Ray ObjectRay = new Ray();


        /* FUNCTIONS */
        public static void DrawGameObject(GameObject Object, GameObject SelectedObject)
        {
            // Draw the object
            RenderUtils.RenderObject(Object, SelectedObject);
        }

        public static void ConfigureCamera()
        {
            Camera.Offset = new Vector2(Editor.EditorWindowSize.Width / 2f, (Editor.EditorWindowSize.Height / 2f) - (Editor.WindowSplits.Height / 3f));
            Camera.Target = Vector2.Zero;
            Camera.Rotation = 0f;
            Camera.Zoom = 1f;
        }
    }
}