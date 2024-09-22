/* DIRECTIVES */
using System.Numerics;
using Hexa.NET.ImGui;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class GUIUtils
    {
        /* VARIABLES */
        public enum WindowElements
        {
            Text,
            Button,
            IntInput,
            FloatInput,
            Vector2Input,
            Vector3Input
        }

        public static List<Action> ImGuiDrawActions = new List<Action>();
        static Vector2 WindowSize = Vector2.Zero;
        static bool IsWindowResized = false;


        /* FUNCTIONS */
        public static void RenderGUI()
        {
            foreach(var DrawAction in ImGuiDrawActions)
            {
                DrawAction();

                /*ConsoleUtils.StatusWrite(DrawAction.Method.Name);

                if (DrawAction.Method.Name.Contains("WindowEnd"))
                {
                    ConsoleUtils.StatusWrite("GUH");
                    WindowSize = ImGui.GetWindowSize();
                }*/
            }

            ImGuiDrawActions.Clear();
        }

        public static void MakeWindow(string Title)
        {
            ImGuiDrawActions.Add(() => {
                ImGui.Begin(Title);
                //ConsoleUtils.StatusWrite(WindowSize.ToString());
            });
        }

        public static void WindowEnd()
        {
            ImGuiDrawActions.Add(() => {
                ImGui.End();
            });
        }

        public static void ResizeNextGUIWindow(Vector2 WindowSize)
        {
            ImGuiDrawActions.Add(() => 
            {
                ImGui.SetNextWindowSize(WindowSize);
                GUIUtils.WindowSize = WindowSize;
            });
        }

        public static void SetGUIWindowPos(Vector2 Position)
        {
            ImGuiDrawActions.Add(() => ImGui.SetWindowPos(Position));
        }

        public static Vector2 GetGUIWindowSize()
        {
            return WindowSize;
        }

        public static object AddWindowElement(WindowElements ElementType, ref object Data, params object[] Arguments)
        {
            switch (ElementType)
            {
                case WindowElements.Text:
                    ImGui.Text(Data.ToString());
                    break;

                case WindowElements.Button:
                    if (ImGui.Button(Data.ToString()))
                    {
                        ImGui.EndChild();
                        return true;
                    }
                    else
                    {
                        ImGui.EndChild();
                        return false;
                    }

                default:
                    throw new ArgumentException($"Invalid element type \"{ElementType.ToString()}\".");

                //case WindowElements.IntInput:
                //    return ImGui.InputInt(Data.ToString(), Arguments[0], Arguments[1], Arguments[2]);
            }

            return true;
        }
    }
}