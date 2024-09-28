/* DIRECTIVES */
using System.Runtime.InteropServices;
using System.Drawing;
using Hexa.NET.ImGuizmo;
using Hexa.NET.Raylib;
using Hexa.NET.ImGui;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class Runtime
    {
        /* VARIABLES */
        public static readonly Size DefaultMinimumWindowSize = new Size(1024, 768);
        public static ImGuiManager GUIManager;
        public static ConfigFlags DefaultConfigFlags = ConfigFlags.FlagWindowResizable | ConfigFlags.FlagVsyncHint | ConfigFlags.FlagWindowAlwaysRun;
        public static Thread PhysicsThread;
        public static float DeltaTime = 0f;
        public static float FrameTime = 0f;
        public static int GCCollectionFreq = 10000;
        public static int FrameCount = 0;


        /* FUNCTIONS */
        // Don't call manually unless absolutely required
        public static void InitGraphics(Size WindowSize, Size MinimumWindowSize, ConfigFlags WindowFlags, string WindowTitle = "New Window")
        {
            ConsoleUtils.StatusWrite("Initializing graphics...");

            // Set the window flags
            ConsoleUtils.StatusWrite("Setting window flags...", ConsoleUtils.StatusTypes.DEBUG);
            Raylib.SetConfigFlags((uint)WindowFlags);

            // Initialize the main window
            ConsoleUtils.StatusWrite("Initializing main window...", ConsoleUtils.StatusTypes.DEBUG);
            Raylib.InitWindow(WindowSize.Width, WindowSize.Height, WindowTitle);
            Raylib.SetWindowMinSize(MinimumWindowSize.Width, MinimumWindowSize.Height);

            // Set window icon based on OS (and arch if the OS is Linux)
            ConsoleUtils.StatusWrite("Detecting current OS and architecture...", ConsoleUtils.StatusTypes.DEBUG);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.X86:
                    case Architecture.X64:
                        Raylib.SetWindowIcon(MaterialTextureImporter.LoadImageFromFile("Resources/Icons/AppIconLinux86_64.png"));
                        break;

                    case Architecture.Arm:
                    case Architecture.Arm64:
                    case Architecture.Armv6:
                        Raylib.SetWindowIcon(MaterialTextureImporter.LoadImageFromFile("Resources/Icons/AppIconLinuxARM.png"));
                        break;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Raylib.SetWindowIcon(MaterialTextureImporter.LoadImageFromFile("Resources/Icons/AppIconWin.png"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Raylib.SetWindowIcon(MaterialTextureImporter.LoadImageFromFile("Resources/Icons/AppIconMacOS.png"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                Raylib.SetWindowIcon(MaterialTextureImporter.LoadImageFromFile("Resources/Icons/AppIconBSD.png"));
            }
            else
            {
                ConsoleUtils.StatusWrite("The OS is not Windows, Linux, MacOS, or FreeBSD. This may cause issues.", ConsoleUtils.StatusTypes.WARNING);
                Raylib.SetWindowIcon(MaterialTextureImporter.LoadImageFromFile("Resources/Icons/Unknown.png"));
            }

            // Load a splash image
            Texture SplashImage = MaterialTextureImporter.LoadTextureFromFile("Resources/Images/Splash.png");

            // Draw the splash screen
            for (int i = 1; i < 32; i++)
            {
                Raylib.BeginDrawing();
                Raylib.DrawTexture(SplashImage, (WindowSize.Width / 2) - (SplashImage.Width / 2), (WindowSize.Height / 2) - (SplashImage.Height / 2), Raylib.White);
                Raylib.DrawRectangle(0, 0, WindowSize.Width, WindowSize.Height, new Hexa.NET.Raylib.Color(0, 0, 0, (byte)(255 - (i * 8))));
                Raylib.EndDrawing();
                Thread.Sleep(10);
            }
        }

        public static void Init(Size WindowSize, string GameName, bool InitializeGraphics = true)
        {
            ConsoleUtils.StatusWrite("Initializing runtime...");

            if (InitializeGraphics == true)
            {
                InitGraphics(WindowSize, DefaultMinimumWindowSize, DefaultConfigFlags, GameName);

                // Print the current monitor's refresh rate
                int CurrentMonitor = Raylib.GetCurrentMonitor();
                ConsoleUtils.StatusWrite($"Current monitor's refresh rate: {Raylib.GetMonitorRefreshRate(CurrentMonitor)}", ConsoleUtils.StatusTypes.DEBUG);
            }

            // Disable the exit key
            ConsoleUtils.StatusWrite("Disabling exit key...", ConsoleUtils.StatusTypes.DEBUG);
            Raylib.SetExitKey((int)KeyboardKey.Null);

            // Initialize audio
            ConsoleUtils.StatusWrite("Initializing audio...", ConsoleUtils.StatusTypes.DEBUG);
            Raylib.InitAudioDevice();

            // Initialize the physics controller
            ConsoleUtils.StatusWrite("Initializing physics controller...", ConsoleUtils.StatusTypes.DEBUG);
            Physics.Init();

            // Initialize the physics thread and update at the target physics framerate
            ConsoleUtils.StatusWrite("Initializing physics thread...", ConsoleUtils.StatusTypes.DEBUG);
            PhysicsThread = new Thread(() => {
                while (Physics.KillPhysicsThread == false)
                {
                    try
                    {
                        if (Physics.PhysicsFPS <= 0f)
                        {
                            Physics.PhysicsFPS = 60f;
                            throw new ArgumentOutOfRangeException("Physics.PhysicsFPS", "Physics FPS must be greater than zero. It will be set to 60.");
                        }

                        Physics.UpdatePhysics();
                    }
                    catch (Exception EX)
                    {
                        ConsoleUtils.StatusWrite($"Physics update error: {EX.Message}", ConsoleUtils.StatusTypes.WARNING);
                    }
                    
                    Thread.Sleep((int)(1000f / Physics.PhysicsFPS));
                }
            });

            PhysicsThread.Start();

            // Initialize ImGui
            ConsoleUtils.StatusWrite("Initializing ImGui...", ConsoleUtils.StatusTypes.DEBUG);
            GUIManager = new ImGuiManager();

            ConsoleUtils.StatusWrite("Setting GUI style...", ConsoleUtils.StatusTypes.DEBUG);
            
            // Set the color scheme
            ImGui.StyleColorsDark();

            ImGuiStylePtr GUIStyle = ImGui.GetStyle();

            // Disable rounding
            GUIStyle.WindowRounding = 0f;
            GUIStyle.ChildRounding = 0f;
            GUIStyle.FrameRounding = 0f;
            GUIStyle.PopupRounding = 0f;
            GUIStyle.GrabRounding = 0f;
            GUIStyle.TabRounding = 0f;

            unsafe
            {
                ImGuizmo.SetImGuiContext(ImGui.GetCurrentContext());
                ImGuizmo.SetOrthographic(false);
            }

            // Draw the splash screen
            for (int i = 0; i < 32; i++)
            {
                Raylib.BeginDrawing();
                Raylib.DrawRectangle(0, 0, WindowSize.Width, WindowSize.Height, new Hexa.NET.Raylib.Color(0, 0, 0, (byte)(i * 2)));
                Raylib.EndDrawing();
                Thread.Sleep(10);
            }
        }
    }
}