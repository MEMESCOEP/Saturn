/* DIRECTIVES */
using System.Runtime.InteropServices;
using System.Drawing;
using Hexa.NET.Raylib;
using Hexa.NET.ImGui;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class EntryPoint
    {
        /* VARIABLES */
        // Public
        public static List<GameObject> ObjectsInScene = new List<GameObject>();
        public static List<Action> ExtraDrawCalls = new List<Action>();
        public static string EngineVersion = "0.106P"; // <Major revision>.<month# + day# + year# + hour(24-hr) + min><Release type (P = prototype, A = alpha, B = beta, RC = release candidate, SR = stable release)>
        public static bool WindowOpened = false;
        public static bool Debug = false;

        // Private
        static string SceneToLoad = "ProjectData/Scenes/MainScene.mscn";


        /* FUNCTIONS */
        public static void Main(string[] Args)
        {
            try
            {
                Console.WriteLine($"[== Saturn Engine {EngineVersion} ==]");
                bool SkipArgument = false;

                foreach(string CMDArg in Args)
                {
                    if (SkipArgument == true)
                    {
                        SkipArgument = false;
                        continue;
                    }

                    switch (CMDArg)
                    {
                        case "--Help":
                            Console.WriteLine("\n");
                            ShowHelp();
                            break;

                        case "--EnableDebug":
                            Debug = true;
                            break;

                        default:
                            ConsoleUtils.StatusWrite($"\"{CMDArg}\" is not a valid argument.\n\n", ConsoleUtils.StatusTypes.ERROR);
                            ShowHelp(-1);
                            break;
                    }
                }

                SDL2.SDL.SDL_GetRenderDriverInfo(0, out var GLInfo);
                
                ConsoleUtils.StatusWrite($"Running on {RuntimeInformation.OSDescription} ({Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.MajorRevision}.{Environment.OSVersion.Version.Minor}.{Environment.OSVersion.Version.MinorRevision}_{RuntimeInformation.OSArchitecture})", ConsoleUtils.StatusTypes.DEBUG);
                ConsoleUtils.StatusWrite($"Raylib version: {Raylib.RAYLIB_VERSION}", ConsoleUtils.StatusTypes.DEBUG);
                ConsoleUtils.StatusWrite($"SDL Version: {SDL2.SDL.SDL_MAJOR_VERSION}.{SDL2.SDL.SDL_MINOR_VERSION}.{SDL2.SDL.SDL_PATCHLEVEL}", ConsoleUtils.StatusTypes.DEBUG);
                ConsoleUtils.StatusWrite($"Video driver: {SDL2.SDL.SDL_GetVideoDriver(0)}", ConsoleUtils.StatusTypes.DEBUG);
                ConsoleUtils.StatusWrite($"H: {GLInfo.name}", ConsoleUtils.StatusTypes.DEBUG);
                ConsoleUtils.StatusWrite($"RLGL version: {Raylib.RLGL_VERSION}", ConsoleUtils.StatusTypes.DEBUG);
                Raylib.SetTraceLogLevel(Debug == true ? (int)TraceLogLevel.All : (int)TraceLogLevel.None);

                ConsoleUtils.StatusWrite("Initializing editor...");
                new Editor().InitEditor();
            }
            catch(Exception EX)
            {
                ConsoleUtils.StatusWrite($"Saturn editor init error: {EX.Message}\n\t{EX.InnerException}", ConsoleUtils.StatusTypes.ERROR);
                ExitEngine(EX.HResult);
            }
        }

        public static void GameInit(int WindowSizeX, int WindowSizeY, string GameName = "New Window", bool EnableDebug = false)
        {
            ConsoleUtils.StatusWrite("Initializing runtime...");
            GameObjectUtils.ObjectList = ObjectsInScene;
            Debug = EnableDebug;

            Runtime.Init(new Size(WindowSizeX, WindowSizeY), GameName);
            SceneImporter.ImportSceneFile(SceneToLoad, ref ObjectsInScene);
            ViewportUtils.ConfigureCamera();
            Physics.Enable = true;
            WindowOpened = true;
        }

        public static void Render()
        {
            try
            {
                Runtime.GUIManager.NewFrame();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Raylib.Black);
                GUIUtils.RenderGUI();

                // Update the mouse properties
                Mouse.UpdateMouseProperties();

                #region 2D shenanigans
                Raylib.BeginMode2D(ViewportUtils.Camera);

                // Render all gameobjects
                for (int ObjectIndex = ObjectsInScene.Count; ObjectIndex > 0; ObjectIndex--)
                {
                    RenderUtils.RenderObject(ObjectsInScene[ObjectIndex - 1]);

                    foreach(GameObject ChildObject in GameObjectUtils.GetAllChildren(ObjectsInScene[ObjectIndex - 1]))
                    {
                        RenderUtils.RenderObject(ChildObject);
                    }
                }

                // Update all gameobjects
                for (int ObjectIndex = ObjectsInScene.Count; ObjectIndex > 0; ObjectIndex--)
                {
                    try
                    {
                        ObjectsInScene[ObjectIndex - 1].Update();
                    }
                    catch (Exception EX)
                    {
                        ConsoleUtils.StatusWrite($"Object update error: {EX.Message}\n\tStack trace -> {EX.StackTrace}", ConsoleUtils.StatusTypes.ERROR);
                    }
                }

                Raylib.EndMode2D();
                #endregion

                foreach (Action DrawCall in ExtraDrawCalls)
                {
                    DrawCall();
                }

                Runtime.GUIManager.EndFrame();
                Raylib.EndDrawing();

                // Update DeltaTime
                Runtime.DeltaTime = Raylib.GetFrameTime();
                Runtime.FrameTime += Runtime.DeltaTime;
                Runtime.FrameCount++;

                // Reset the frame time once it reaches 1 second
                if (Runtime.FrameTime >= 1f)
                {
                    Runtime.FrameTime = 0;
                }
            }
            catch (Exception EX)
            {
                ConsoleUtils.StatusWrite($"Render error: {EX.Message}", ConsoleUtils.StatusTypes.ERROR);
            }
        }

        public static void ExitEngine(int StatusCode = 0)
        {
            ConsoleUtils.StatusWrite("Cleaning up...");

            if (StatusCode != 0)
            {
                ConsoleUtils.StatusWrite($"Something failed and called ExitEngine with a status code of {StatusCode}!", ConsoleUtils.StatusTypes.WARNING);
            }

            // Close everything (Audio devices, windows, ImGui, etc)
            #region Close everything
            ConsoleUtils.StatusWrite("Stopping physics...", ConsoleUtils.StatusTypes.DEBUG);
            Physics.StopPhysics();

            ConsoleUtils.StatusWrite("Unloading all threaded resources...", ConsoleUtils.StatusTypes.DEBUG);
            ThreadedResourceImporter.UnloadAllResources();

            if (WindowOpened == true)
            {
                ConsoleUtils.StatusWrite("Closing audio device...", ConsoleUtils.StatusTypes.DEBUG);
                Raylib.CloseAudioDevice();

                ConsoleUtils.StatusWrite("Closing ImGui...", ConsoleUtils.StatusTypes.DEBUG);
                ImGui.Shutdown();

                ConsoleUtils.StatusWrite("Closing window...", ConsoleUtils.StatusTypes.DEBUG);
                Raylib.CloseWindow();
            }
            #endregion

            ConsoleUtils.StatusWrite("Cleanup finished.", ConsoleUtils.StatusTypes.DEBUG);
            Environment.Exit(StatusCode);
        }

        private static void ShowHelp(int StatusCode = 0)
        {
            Console.WriteLine("╔═══════════════════╣ Saturn Help ╠══════════════════╗");
            Console.WriteLine("║       Command                 Description          ║");
            Console.WriteLine("╠════════════════════════════════════════════════════╣");
            Console.WriteLine("║ 1. --Help                 Shows this help box      ║");
            Console.WriteLine("║ 2. --EnableDebug         Enables debug logging     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
            Environment.Exit(StatusCode);
        }
    }    
}