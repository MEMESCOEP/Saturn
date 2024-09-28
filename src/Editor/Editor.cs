/* DIRECTIVES */
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Numerics;
using IniParser.Model;
using IniParser;
using Box2D.NET.Bindings;
using Hexa.NET.Raylib;
using Hexa.NET.ImGui;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class Editor
    {
        /* VARIABLES */
        // Public
        public static System.Drawing.Size InitialEditorSize = new System.Drawing.Size(1280, 720);
        public static System.Drawing.Size EditorWindowSize = new System.Drawing.Size(1280, 720);
        public static System.Drawing.Size WindowSplits = new System.Drawing.Size(0, 0);
        public static List<GameObject> GameObjectsInScene = new List<GameObject>();
        public static List<string> ScenesInProject = new List<string>();
        public static string BackgroundOperationText = "Please wait while the current operation finishes";
        public static string ProjectConfigPath = "Unknown";
        public static string ProjectConfigFile = "Unknown";
        public static string CurrentScene = "New Scene";
        public static string ProjectName = "Empty Project";
        public static float BackgroundOperationProgress = 0f;
        public static bool BackgroundOperationRunning = false;

        // Private
        // Readonly
        private readonly ConfigFlags WindowFlags = ConfigFlags.FlagWindowResizable | ConfigFlags.FlagVsyncHint | ConfigFlags.FlagWindowAlwaysRun;
        private readonly string[] RBTypes = new string[] { "Static", "Dynamic", "Kinematic" };
        private readonly int InactiveFPS = 10;

        // Changeable
        private System.Drawing.Size ViewportSize = new System.Drawing.Size(0, 0);
        private FileIniDataParser CFGParser = new FileIniDataParser();
        private RenderTexture ViewportRenderTexture;
        private GameObject SelectedObject = null;
        private Vector2 ViewportPosition = new Vector2(0, 32);
        private Vector2 EditorWindowPos = Vector2.Zero;
        private Vector2 SelectionOffset = Vector2.Zero;
        private IniData ConfigData = new IniData();
        private List<ProjectDirectory> DirectoriesInProject = new List<ProjectDirectory>();
        private List<ProjectFile> FilesInProject = new List<ProjectFile>();
        private List<Icon> Icons = new List<Icon>();
        private Icon UpDirIcon = new Icon();
        private string CurrentWindowState = "Not maximized";
        private string CurrentMonitorName = "Unknown";
        private string CurrentDirectory = String.Empty;
        private float NextFrameTime = 0;
        private float FrameTime = 0;
        private bool ViewportKeyboardShortcutsEnabled = true;
        private bool AdvancedRotationEdit = false;
        private bool PublishWindowSetPos = true;
        private bool DrawNewObjectMenu = false;
        private bool ShowPublishWindow = false;
        private bool ConfigExists = false;
        private bool AlwaysRun = false;
        private int MonitorRefreshRate = 60;
        private int CurrentMonitor = 0;
        private int FPSTarget = 60;
        private int FPS = 0;


        /* FUNCTIONS */
        public void InitEditor()
        {       
            try 
            {
                // Update the current directory
                ConsoleUtils.StatusWrite("Getting current directory...");
                CurrentDirectory = Directory.GetCurrentDirectory();

                if (ProjectConfigPath == "Unknown")
                {
                    ProjectConfigPath = CurrentDirectory;
                }
                
                // Initialize graphics
                Runtime.InitGraphics(EditorWindowSize, Runtime.DefaultMinimumWindowSize, WindowFlags, $"Saturn {EntryPoint.EngineVersion} || Empty Project || (0 FPS)");
                
                // Get the current monitor's name
                ConsoleUtils.StatusWrite("Getting current monitor name...", ConsoleUtils.StatusTypes.DEBUG);
                CurrentMonitorName = Raylib.GetMonitorNameS(CurrentMonitor);

                // Get the current monitor's refresh rate
                ConsoleUtils.StatusWrite("Getting monitor refresh rate...", ConsoleUtils.StatusTypes.DEBUG);
                MonitorRefreshRate = Raylib.GetMonitorRefreshRate(CurrentMonitor);

                // If the config file exists, set the window monitor position, and window state
                if (ConfigExists == true)
                {                    
                    ConsoleUtils.StatusWrite($"Setting window monitor to {CurrentMonitor} (\"{CurrentMonitorName}\")...", ConsoleUtils.StatusTypes.DEBUG);
                    Raylib.SetWindowMonitor(CurrentMonitor);
                    
                    ConsoleUtils.StatusWrite("Setting window position...", ConsoleUtils.StatusTypes.DEBUG);
                    Raylib.SetWindowPosition((int)EditorWindowPos.X, (int)EditorWindowPos.Y);
                    
                    if (CurrentWindowState == "Maximized")
                    {
                        Raylib.MaximizeWindow();
                    }
                }

                EntryPoint.WindowOpened = true;

                // Load icons from the disk
                ConsoleUtils.StatusWrite("Loading icons...");

                foreach (string IconFile in Directory.GetFiles("Resources/Icons"))
                {
                    Icon NewIcon = new Icon();

                    if (Path.GetExtension(IconFile) != ".png")
                    {
                        ConsoleUtils.StatusWrite($"File \"{IconFile}\" is not an icon, or it may not be a PNG image.", ConsoleUtils.StatusTypes.WARNING);
                        continue;
                    }

                    ConsoleUtils.StatusWrite($"Loading icon \"{IconFile}\"...", ConsoleUtils.StatusTypes.DEBUG);                    
                    NewIcon.IconTexture = MaterialTextureImporter.LoadTextureFromFile(IconFile);
                    NewIcon.IconName = Path.GetFileNameWithoutExtension(IconFile);

                    GCHandle TextureHandle = GCHandle.Alloc(NewIcon.IconTexture, GCHandleType.Pinned);
                    NewIcon.IconID = new ImTextureID(TextureHandle.AddrOfPinnedObject());

                    Icons.Add(NewIcon);
                }

                UpDirIcon = Icons.Where(i => i.IconName == "DirectoryUp").FirstOrDefault();
                
                // Initialize the runtime
                Runtime.Init(EditorWindowSize, "Saturn", false);

                ViewportRenderTexture = Raylib.LoadRenderTexture(1, 1);
                UpdateWindowProperties();
                WindowSplits.Width = EditorWindowSize.Width / 6;
                WindowSplits.Height = EditorWindowSize.Height / 3;
                
                // Configure the camera
                ViewportUtils.ConfigureCamera();

                // Find all files in the project directory
                ProjectFSItemImporter.Init(ProjectConfigPath, FilesInProject, DirectoriesInProject, Icons);             

                ConsoleUtils.StatusWrite("Init finished.");
                InitialEditorSize = new System.Drawing.Size(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                WindowLoop();
            }
            catch (Exception EX)
            {
                ConsoleUtils.StatusWrite($"Init error: {EX.Message}\n\tStack trace: {EX.StackTrace}", ConsoleUtils.StatusTypes.ERROR);
                Messagebox.ShowMessage("Saturn - ERROR", $"An error occurred during initialization.\n\nDetails:\n  Error message: {EX.Message}\n  Error code: {EX.HResult}\n  Stack trace:\n    {EX.StackTrace}", SDL2.SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR);
                ExitEditor(false, EX.HResult);
            }
        }

        private void WindowLoop()
        {
            try
            {
                // This is the main window loop. It will be run infinitely until the window should close.
                while (!Raylib.WindowShouldClose())
                {
                    // Get the engine FPS and update the mouse properties and camera every frame
                    #region Start of frame shenanigans
                    FPS = Raylib.GetFPS();
                    Mouse.UpdateMouseProperties();
                    Runtime.GUIManager.NewFrame();
                    Raylib.BeginDrawing();
                    ViewportKeyboardShortcutsEnabled = !ImGui.IsAnyItemActive();
                    ThreadedResourceImporter.ProcessResourcesOpenGL();
                    #endregion

                    // Call every game object's script update method
                    CallUpdateOnGameObjects();

                    // Only update the GUI if the window is focused
                    #region GUI Update
                    if (Raylib.IsWindowFocused() == true || AlwaysRun == true)
                    {
                        // Update the window properties every 250 milliseconds
                        if (FrameTime >= NextFrameTime)
                        {
                            UpdateWindowProperties();

                            WindowSplits.Width = EditorWindowSize.Width / 6;
                            WindowSplits.Height = EditorWindowSize.Height / 3;

                            Raylib.SetWindowTitle($"Saturn {EntryPoint.EngineVersion} || {ProjectName} - {Path.GetFileNameWithoutExtension(CurrentScene)} || {FPS} FPS");
                            NextFrameTime = NextFrameTime + 0.25f;
                        }
                        
                        // Set the framerate to the monitor's refresh rate if the window was just focused
                        if (FPSTarget != MonitorRefreshRate)
                        {
                            Raylib.SetTargetFPS(MonitorRefreshRate);
                            FPSTarget = MonitorRefreshRate;
                        }

                        // Clear the background
                        Raylib.ClearBackground(EngineTheme.BGColor);

                        // Render the scene
                        #region Render the scene
                        Raylib.BeginMode2D(ViewportUtils.Camera);

                        // Draw the editing grid                       
                        Raylib.DrawLine(-10000, 0, 10000, 0, Raylib.Red);
                        Raylib.DrawLine(0, -10000, 0, 10000, Raylib.Green);

                        // Draw game objects
                        #region Draw game objects
                        foreach (GameObject Object in GameObjectsInScene)
                        {
                            ViewportUtils.DrawGameObject(Object, SelectedObject);

                            if (Object.Children.Count > 0)
                            {
                                foreach (GameObject Child in GameObjectUtils.GetAllChildren(Object))
                                {
                                    ViewportUtils.DrawGameObject(Child, SelectedObject);
                                }
                            }
                        }
                        #endregion

                        // Draw the utility controls
                        #region Draw selected object utility controls
                        if (SelectedObject != null)
                        {
                            //ViewportUtils.DrawUtilityControls(SelectedObject);
                        }
                        #endregion

                        Raylib.EndMode2D();
                        #endregion

                        // Draw the menubar
                        #region Draw the menubar
                        if (ImGui.BeginMainMenuBar())
                        {
                            if (ImGui.BeginMenu("File"))
                            {
                                if (ImGui.MenuItem("New project", "Ctrl+N"))
                                {
                                    EditorActions.CreateProject(ref ProjectName, ref ProjectConfigPath, ref ProjectConfigFile);
                                }

                                if (ImGui.MenuItem("Open project", "Ctrl+O"))
                                {
                                    EditorActions.SelectProjectFromDisk(ref GameObjectsInScene, ref ScenesInProject);
                                }

                                if (ImGui.MenuItem("Save project", "Ctrl+S"))
                                {
                                    EditorActions.SaveProjectToDisk(ProjectName, ProjectConfigPath, ProjectConfigFile, CurrentScene, ScenesInProject);
                                }

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                if (ImGui.MenuItem("Open scene", "Ctrl+Shift+O") || Keyboard.IsKeyPressed(KeyboardKey.O))
                                {
                                    EditorActions.SelectSceneFromDisk(GameObjectsInScene, ScenesInProject);
                                }

                                if (ImGui.MenuItem("Save scene", "Ctrl+Shift+S"))
                                {
                                    EditorActions.SaveSceneToDisk(CurrentScene, GameObjectsInScene);
                                }

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                if (ImGui.MenuItem("Exit"))
                                {
                                    ExitEditor(true, 0);
                                }

                                ImGui.EndMenu();
                            }

                            if (ImGui.BeginMenu("Edit"))
                            {
                                if (ImGui.MenuItem("Preferences"))
                                {
                                    Messagebox.ShowMessage("Test", "This is a test msgbox", SDL2.SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION);
                                }

                                ImGui.EndMenu();
                            }

                            if (ImGui.BeginMenu("Object"))
                            {
                                if (ImGui.BeginMenu("Create new"))
                                {
                                    if (ImGui.MenuItem("Basic"))
                                    {
                                        
                                    }

                                    if (ImGui.MenuItem("Empty"))
                                    {
                                        
                                    }

                                    ImGui.EndMenu();
                                }

                                ImGui.EndMenu();
                            }

                            if (ImGui.BeginMenu("Build"))
                            {
                                if (ImGui.MenuItem("Run test build", "F5"))
                                {
                                    RuntimeBuilder.RunTestBuild();
                                }

                                if (ImGui.MenuItem("Publish", "CTRL+B"))
                                {
                                    PublishWindowSetPos = true;
                                    ShowPublishWindow = true;
                                }

                                ImGui.EndMenu();
                            }

                            ImGui.EndMainMenuBar();
                            Raylib.DrawRectangleLines(WindowSplits.Width * 2, 16, EditorWindowSize.Width - (WindowSplits.Width * 2), 16, EngineTheme.SubWinBorderColor);
                        }                        
                        #endregion

                        // Draw the add object menu
                        #region Draw the add object menu
                        if (DrawNewObjectMenu == true)
                        {
                            ImGui.SetNextWindowPos(ViewportUtils.AddNewObjectWinPos);
                            ImGui.Begin("Add new object", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
                            ImGui.SetWindowSize(new Vector2(320, 200));
                            ImGui.TextUnformatted("No scripts.");
                            ImGui.End();
                        }
                        #endregion

                        // Draw the scene manager
                        #region Draw the scene manager

                        int ObjectsInScene = 0;

                        foreach(var Object in GameObjectsInScene)
                        {
                            ObjectsInScene += 1 + Object.Children.Count;

                            foreach (var ChildObject in Object.Children)
                            {
                                ObjectsInScene +=  ChildObject.Children.Count;
                            }
                        }

                        Raylib.DrawRectangle(0, 16, WindowSplits.Width, 32, EngineTheme.SubWinTitleColor);
                        Raylib.DrawRectangle(0, 48, WindowSplits.Width, EditorWindowSize.Height, EngineTheme.SubWinColor);
                        Raylib.DrawRectangleLines(0, 16, WindowSplits.Width, EditorWindowSize.Height, EngineTheme.SubWinBorderColor);
                        Raylib.DrawText("Scene", 8, 24, 20, EngineTheme.TextColor);
                        ImGui.SetNextWindowPos(new Vector2(8, 56));
                        ImGui.Begin($"Objects ({ObjectsInScene} in scene)", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
                        ImGui.SetWindowSize(new Vector2(WindowSplits.Width - 16, EditorWindowSize.Height - 64));
                        ImGui.Spacing();

                        foreach (GameObject Object in GameObjectsInScene)
                        {
                            ImGui.PushID(GameObjectsInScene.IndexOf(Object));

                            if (ImGui.CollapsingHeader(Object.ObjectName))
                            {
                                if (Object.Children.Count > 0)
                                {                                 
                                    GameObjectUtils.RecursiveChildList(Object);
                                }
                                else
                                {
                                    ImGui.Indent(16f);
                                    ImGui.TextUnformatted("No children.");
                                    ImGui.Unindent(16f);
                                }
                            }

                            ImGui.PopID();
                        }

                        ImGui.End();
                        #endregion

                        // Draw the game object property manager
                        #region Draw the game object property manager
                        Raylib.DrawRectangle(EditorWindowSize.Width - WindowSplits.Width, 16, WindowSplits.Width, 32, EngineTheme.SubWinTitleColor);
                        Raylib.DrawRectangle(EditorWindowSize.Width - WindowSplits.Width, 48, WindowSplits.Width, EditorWindowSize.Height, EngineTheme.SubWinColor);
                        Raylib.DrawRectangleLines(EditorWindowSize.Width - WindowSplits.Width, 16, WindowSplits.Width, EditorWindowSize.Height, EngineTheme.SubWinBorderColor);
                        Raylib.DrawText("Properties", EditorWindowSize.Width - WindowSplits.Width + 8, 24, 20, EngineTheme.TextColor);

                        // Object properties window
                        ImGui.SetNextWindowPos(new Vector2(EditorWindowSize.Width - WindowSplits.Width + 8, 56));
                        ImGui.Begin("Object properties", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
                        ImGui.SetWindowSize(new Vector2(WindowSplits.Width - 16, EditorWindowSize.Height - 64));

                        if (SelectedObject != null)
                        {
                            ImGui.PushID(0);
                            
                            if (ImGui.CollapsingHeader("Name"))
                            {
                                ImGui.InputText("Text", ref SelectedObject.ObjectName, 99999);
                                
                                for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                    ImGui.Spacing();
                            }

                            ImGui.PopID();
                            ImGui.PushID(1);

                            if (ImGui.CollapsingHeader("Transform"))
                            {
                                Vector2 NewPosition = PhysicsRaylibConversions.Box2DToRaylib(SelectedObject.ObjectRigidbody.position);
                                float Rotation = MathUtils.Rad2Deg(MathUtils.MakeAngleFromRot(SelectedObject.ObjectRigidbody.rotation));

                                ImGui.Checkbox("Is object visible", ref SelectedObject.IsVisible);

                                for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                    ImGui.Spacing();

                                ImGui.TextUnformatted("Position (Vector2):");
                                ImGui.PushID(0);
                                ImGui.InputFloat2("X, Y", ref NewPosition);

                                SelectedObject.ObjectRigidbody.position = PhysicsRaylibConversions.RaylibToBox2D(NewPosition);
                                ImGui.PopID();

                                for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                    ImGui.Spacing();

                                ImGui.TextUnformatted("Rotation:");
                                ImGui.Checkbox("Use advanced rotation", ref AdvancedRotationEdit);

                                SelectedObject.ObjectRigidbody.fixedRotation = Convert.ToByte(true);

                                if (AdvancedRotationEdit == true)
                                {
                                    ImGui.PushID(1);
                                    ImGui.InputFloat("Sin", ref SelectedObject.ObjectRigidbody.rotation.s);
                                    ImGui.PopID();

                                    ImGui.PushID(2);
                                    ImGui.InputFloat("Cos", ref SelectedObject.ObjectRigidbody.rotation.c);
                                    ImGui.PopID();
                                }
                                else
                                {
                                    ImGui.PushID(1);
                                    ImGui.InputFloat("Degrees", ref Rotation);

                                    SelectedObject.ObjectRigidbody.rotation.c = (float)Math.Cos(MathUtils.Deg2Rad(Rotation));
                                    SelectedObject.ObjectRigidbody.rotation.s = (float)Math.Sin(MathUtils.Deg2Rad(Rotation));

                                    ImGui.PopID();
                                }

                                for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                    ImGui.Spacing();

                                ImGui.TextUnformatted("Scale (Vector2):");
                                ImGui.PushID(3);
                                ImGui.InputFloat2("X, Y", ref SelectedObject.Scale);
                                ImGui.PopID();

                                for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                    ImGui.Spacing();
                            }

                            ImGui.PopID();
                            ImGui.PushID(2);
                            
                            if (ImGui.CollapsingHeader("Physics"))
                            {
                                ImGui.Indent(16f);

                                if (ImGui.CollapsingHeader("Rigidbody"))
                                {
                                    bool FastRotation = Convert.ToBoolean(SelectedObject.ObjectRigidbody.allowFastRotation);
                                    bool AllowSleep = Convert.ToBoolean(SelectedObject.ObjectRigidbody.enableSleep);

                                    ImGui.Indent(16f);
                                    ImGui.Checkbox("Rigidbody is static", ref SelectedObject.IsStatic);
                                    ImGui.Checkbox("Allow fast rotation", ref FastRotation);
                                    ImGui.Checkbox("Allow sleeping", ref AllowSleep);

                                    if(AllowSleep == true)
                                    {
                                        ImGui.Indent(16f);
                                        ImGui.TextUnformatted("Sleep threshold:");
                                        ImGui.PushID(1);
                                        ImGui.InputFloat("", ref SelectedObject.ObjectRigidbody.sleepThreshold);
                                        ImGui.PopID();
                                        ImGui.Unindent(16f);
                                    }

                                    for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                        ImGui.Spacing();
                                        
                                    ImGui.TextUnformatted("Rigidbody type:");
                                    ImGui.PushID(2);

                                    unsafe
                                    {
                                        //ImGui.Combo("Type", &CurrentRBType, RBTypes);
                                    }
                                    
                                    ImGui.PopID();
                                    ImGui.Unindent(16f);

                                    for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                        ImGui.Spacing();

                                    SelectedObject.ObjectRigidbody.allowFastRotation = Convert.ToByte(FastRotation);
                                    SelectedObject.ObjectRigidbody.enableSleep = Convert.ToByte(AllowSleep);
                                }                                

                                if (ImGui.CollapsingHeader("World"))
                                {
                                    B2.MassData RBMassData = B2.BodyGetMassData(SelectedObject.ObjectRigidbodyID);

                                    ImGui.Indent(16f);
                                    ImGui.TextUnformatted("Gravity scale:");
                                    ImGui.PushID(0);
                                    ImGui.InputFloat("Y Axis", ref SelectedObject.ObjectRigidbody.gravityScale);
                                    ImGui.PopID();

                                    for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                        ImGui.Spacing();

                                    ImGui.TextUnformatted("Linear damping:");
                                    ImGui.PushID(1);
                                    ImGui.InputFloat("", ref SelectedObject.ObjectRigidbody.linearDamping);
                                    ImGui.PopID();

                                    for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                        ImGui.Spacing();

                                    ImGui.TextUnformatted("Angular damping:");
                                    ImGui.PushID(2);
                                    ImGui.InputFloat("", ref SelectedObject.ObjectRigidbody.angularDamping);
                                    ImGui.PopID();

                                    for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                        ImGui.Spacing();

                                    ImGui.TextUnformatted("Mass:");
                                    ImGui.PushID(2);
                                    ImGui.InputFloat("Kg", ref RBMassData.mass);
                                    ImGui.PopID();
                                    ImGui.Unindent(16f);

                                    for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                        ImGui.Spacing();

                                    B2.BodySetMassData(SelectedObject.ObjectRigidbodyID, RBMassData);
                                }

                                ImGui.Unindent(16f);                

                                for (int SpacingCount = 0; SpacingCount <= 2; SpacingCount++)
                                    ImGui.Spacing();                                
                            }

                            ImGui.PopID();
                            ImGui.PushID(3);
                            
                            try
                            {
                                if (ImGui.CollapsingHeader("Material"))
                                {
                                    ImGui.Indent(16f);
                                    
                                    unsafe
                                    {
                                        ImGui.Columns(2);
                                        GCHandle TextureHandle = GCHandle.Alloc(SelectedObject.ObjectTexture, GCHandleType.Pinned);

                                        ImGui.Image(new ImTextureID(TextureHandle.AddrOfPinnedObject()), new Vector2(64, 64));
                                        TextureHandle.Free();

                                        ImGui.NextColumn();
                                        ImGui.Spacing();

                                        if (ImGui.Button("Change"))
                                        {
                                            SelectedObject.ObjectTexture = MaterialTextureImporter.OpenTextureFileBrowser(SelectedObject.ObjectTexture, SelectedObject);
                                        }

                                        if (ImGui.Button("Save"))
                                        {
                                            MaterialTextureExporter.SaveTextureToFile(SelectedObject.ObjectTexture);
                                        }

                                        ImGui.TextWrapped($"Path: {SelectedObject.ObjectTexturePath}");
                                        ImGui.NextColumn();
                                    }

                                    ImGui.Unindent(16f);
                                    ImGui.Spacing();
                                }
                            }
                            catch
                            {
                                ImGui.TextUnformatted("This object doesn't have a texture.");
                            }

                            ImGui.PopID();
                        }
                        else
                        {
                            ImGui.TextUnformatted("No object selected.");
                        }
                        
                        ImGui.End();
                        #endregion

                        // Draw the filesystem dock
                        #region Draw the filesystem dock
                        Raylib.DrawRectangle(WindowSplits.Width, EditorWindowSize.Height - WindowSplits.Height, EditorWindowSize.Width - (WindowSplits.Width * 2), 32, EngineTheme.SubWinTitleColor);
                        Raylib.DrawRectangle(WindowSplits.Width, EditorWindowSize.Height - WindowSplits.Height + 32, EditorWindowSize.Width - (WindowSplits.Width * 2), EditorWindowSize.Height / 3, EngineTheme.SubWinColor);
                        Raylib.DrawRectangleLines(WindowSplits.Width, EditorWindowSize.Height - WindowSplits.Height, EditorWindowSize.Width - (WindowSplits.Width * 2), EditorWindowSize.Height / 3, EngineTheme.SubWinBorderColor);
                        Raylib.DrawText("Filesystem", WindowSplits.Width + 8, EditorWindowSize.Height - WindowSplits.Height + 8, 20, EngineTheme.TextColor);
                        ImGui.SetNextWindowPos(new Vector2(WindowSplits.Width + 8, EditorWindowSize.Height - WindowSplits.Height + 40));

                        if (ImGui.Begin($"Content ({CurrentDirectory})", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
                        {
                            ImGui.SetWindowSize(new Vector2(EditorWindowSize.Width - (WindowSplits.Width * 2) - 16, WindowSplits.Height - 48));
                            ImGui.Columns(Math.Max((int)(ImGui.GetWindowSize().X / 88), 1), "File list", false); 
                            
                            if (CurrentDirectory != ProjectConfigPath)
                            {
                                if (ImGui.ImageButton("Go up one directory", UpDirIcon.IconID,  new Vector2(64, 64)))
                                {
                                    DirectoryInfo ParentDirectory = Directory.GetParent(CurrentDirectory);

                                    if (ParentDirectory != null)
                                    {
                                        CurrentDirectory = ParentDirectory.FullName;
                                    }
                                }

                                if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                                {
                                    ImGui.BeginTooltip();
                                    ImGui.Text("Click to go back one directory.");
                                    ImGui.EndTooltip();
                                }

                                ImGui.Text("Go back");
                                ImGui.NextColumn();
                            }

                            if (ProjectFSItemImporter.IsFinding == false)
                            {
                                // Display directories
                                foreach (ProjectDirectory Dir in DirectoriesInProject)
                                {                                    
                                    if (Path.GetDirectoryName(Dir.DirInformation.FullName) != CurrentDirectory)
                                    {
                                        continue;
                                    }

                                    ImGui.PushID(DirectoriesInProject.IndexOf(Dir));
                                    
                                    if (ImGui.ImageButton(Dir.DirInformation.Name, Dir.AssociatedIcon.IconID, new Vector2(64, 64)))
                                    {
                                        CurrentDirectory = Dir.DirInformation.FullName;
                                    }

                                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                                    {
                                        ImGui.BeginTooltip();
                                        ImGui.Text($"{Dir.DirInformation.Name}\nClick to enter this directory.");
                                        ImGui.EndTooltip();
                                    }

                                    ImGui.TextUnformatted(Dir.DirInformation.Name);
                                    ImGui.PopID();
                                    ImGui.NextColumn();
                                }

                                // Display files
                                foreach (ProjectFile File in FilesInProject)
                                {
                                    if (File.FileInformation == null || File.AssociatedIcon == null)
                                    {
                                        continue;
                                    }

                                    if (Path.GetDirectoryName(File.FileInformation.FullName) != CurrentDirectory)
                                    {
                                        continue;
                                    }

                                    ImGui.PushID(FilesInProject.IndexOf(File));
                                    
                                    if (ImGui.ImageButton(File.FileInformation.Name, File.AssociatedIcon.IconID, new Vector2(64, 64)))
                                    {
                                        using Process FileOpener = new Process();

                                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                                        {
                                            FileOpener.StartInfo.FileName = "explorer";
                                            FileOpener.StartInfo.Arguments = "/select," + File.FileInformation.FullName + "\"";
                                            FileOpener.Start();
                                        }
                                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                                        {
                                            FileOpener.StartInfo.FileName = "explorer";
                                            FileOpener.StartInfo.Arguments = "-R " + File.FileInformation.FullName;
                                            FileOpener.Start();
                                        }
                                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                                        {
                                            FileOpener.StartInfo = new ProcessStartInfo
                                            {
                                                FileName = "dbus-send",
                                                Arguments = "--print-reply --dest=org.freedesktop.FileManager1 /org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems array:string:\"" + new Uri(File.FileInformation.FullName).ToString() + "\" string:\"\"",
                                                UseShellExecute = true
                                            };

                                            FileOpener.Start();
                                        }
                                    }

                                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                                    {
                                        ImGui.BeginTooltip();
                                        ImGui.Text($"{File.FileInformation.Name}\nClick to show this file in your file manager.");
                                        ImGui.EndTooltip();
                                    }

                                    if (File.FileInformation.Name.Length > 8)
                                    {
                                        ImGui.TextUnformatted(File.FileInformation.Name.Substring(0, 8) + "...");
                                    }
                                    else
                                    {
                                        ImGui.TextUnformatted(File.FileInformation.Name);
                                    }
                                    
                                    ImGui.PopID();
                                    ImGui.NextColumn();
                                }
                            }
                        }
                        
                        ImGui.End();
                        #endregion

                        // Draw the viewport                        
                        #region Draw the viewport
                        Raylib.DrawRectangle(WindowSplits.Width, 16, EditorWindowSize.Width - (WindowSplits.Width * 2), 32, EngineTheme.SubWinTitleColor);
                        Raylib.DrawRectangleLines(WindowSplits.Width, 16, EditorWindowSize.Width - (WindowSplits.Width * 2), 32, EngineTheme.SubWinBorderColor);
                        Raylib.DrawText($"Viewport (X={Math.Round(ViewportUtils.Camera.Target.X, 1)}, Y={Math.Round(ViewportUtils.Camera.Target.Y, 1)}, Zoom={Math.Round(ViewportUtils.Camera.Zoom * 100f, 1)}%)", WindowSplits.Width + 8, 24, 20, EngineTheme.TextColor);
                        #endregion

                        // Draw the publish window
                        if (ShowPublishWindow == true)
                        {
                            ImGui.Begin("Publish Project", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoSavedSettings);
                            
                            if (PublishWindowSetPos == true)
                            {
                                ImGui.SetWindowSize(new Vector2(640, 480));
                                ImGui.SetWindowPos(new Vector2((EditorWindowSize.Width / 2f) - (ImGui.GetWindowSize().X / 2f), (EditorWindowSize.Height / 2f) - (ImGui.GetWindowSize().Y / 2f)));
                                
                                if (Runtime.FrameCount % 10 == 0)
                                {
                                    PublishWindowSetPos = false;
                                }                                
                            }
                            else
                            {
                                if (ImGui.GetWindowSize().X < 320)
                                {
                                    ImGui.SetWindowSize(new Vector2(320, ImGui.GetWindowSize().Y));
                                }
                                
                                if (ImGui.GetWindowSize().Y < 200)
                                {
                                    ImGui.SetWindowSize(new Vector2(ImGui.GetWindowSize().X, 200));
                                }
                            }

                            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowSize().X - 120, ImGui.GetWindowSize().Y - 26));

                            if (ImGui.Button("Publish") == true)
                            {
                                ShowPublishWindow = false;
                                RuntimeBuilder.Compile();
                            }

                            ImGui.SameLine();

                            if (ImGui.Button("Close") == true)
                            {
                                ShowPublishWindow = false;
                            }

                            ImGui.End();
                        }

                        if (BackgroundOperationRunning == false)
                        {
                            // Handle mouse controls
                            #region Handle mouse controls
                            if (MathUtils.IsPointInsideRectangle(Mouse.MouseClickPosition, ViewportPosition, new Vector2(ViewportSize.Width, ViewportSize.Height)) == true)
                            {
                                // Drag an object
                                #region Drag a game object
                                if (Raylib.IsMouseButtonDown((int)MouseButton.Left) == true)
                                {
                                    if (SelectedObject != null)
                                    {
                                        if (Mouse.MousePosition.X <= ViewportPosition.X)
                                        {
                                            Raylib.SetMousePosition((int)ViewportPosition.X + ViewportSize.Width - 2, (int)Mouse.MousePosition.Y);
                                            Mouse.MouseDelta.X = 0;
                                        }
                                        else if (Mouse.MousePosition.X >= ViewportPosition.X + ViewportSize.Width)
                                        {
                                            Raylib.SetMousePosition((int)ViewportPosition.X, (int)Mouse.MousePosition.Y);
                                            Mouse.MouseDelta.X = 0;
                                        }
                                        else if (Mouse.MousePosition.Y <= ViewportPosition.Y)
                                        {
                                            Raylib.SetMousePosition((int)Mouse.MousePosition.X, (int)ViewportPosition.Y + ViewportSize.Height - 2);
                                            Mouse.MouseDelta.Y = 0;
                                        }                                    
                                        else if (Mouse.MousePosition.Y >= ViewportPosition.Y + ViewportSize.Height)
                                        {
                                            Raylib.SetMousePosition((int)Mouse.MousePosition.X, (int)ViewportPosition.Y);
                                            Mouse.MouseDelta.Y = 0;
                                        }

                                        // Snap rigidbody to grid if the control key is pressed
                                        /*if (Raylib.IsKeyDown((int)KeyboardKey.LeftControl) == true)
                                        {
                                            Vector3 RoundingVector = new Vector3(RayDirection == RayDirections.X ? 1 : 0, RayDirection == RayDirections.Y ? 1 : 0, RayDirection == RayDirections.Z ? 1 : 0);
                                            SelectedObject.ObjectRigidbody.Position = PhysicsRaylibConversions.Vector2ToBox2D(MathUtils.RoundVector3(PhysicsRaylibConversions.JVectorToVector3(SelectedObject.ObjectRigidbody.Position), RoundingVector));
                                        }*/
                                    }
                                }
                                else
                                {
                                    if (Raylib.IsMouseButtonDown((int)MouseButton.Middle) == true)
                                    {
                                        if (Raylib.IsCursorHidden() == false)
                                        {
                                            Raylib.DisableCursor();
                                        }

                                        ViewportUtils.Camera.Target -= Mouse.MouseDelta;
                                    }
                                    else
                                    {
                                        // Adjust camera zoom (clamp between 0.1 and 10, 10% <-> 1000%)
                                        ViewportUtils.Camera.Zoom += Mouse.WheelDelta * 0.05f;

                                        if (ViewportUtils.Camera.Zoom > 10.0f)
                                        {
                                            ViewportUtils.Camera.Zoom = 10.0f;
                                        }
                                        else if (ViewportUtils.Camera.Zoom < 0.1f) 
                                        {
                                            ViewportUtils.Camera.Zoom = 0.1f;
                                        }
                                        
                                        if (Raylib.IsCursorHidden() == true)
                                        {
                                            Raylib.EnableCursor();
                                        }
                                    }
                                }
                                #endregion
                                
                                // Pick a game object in 2D space
                                #region Pick a game object
                                if (Raylib.IsMouseButtonPressed((int)MouseButton.Left) == true)
                                {
                                    foreach (GameObject Object in GameObjectsInScene)
                                    {
                                        foreach (GameObject Child in GameObjectUtils.GetAllChildren(Object))
                                        {
                                            if (MathUtils.IsPointInsideRectangle(Raylib.GetScreenToWorld2D(Mouse.MousePosition, ViewportUtils.Camera) + new Vector2((Child.ObjectTexture.Width / 2f) * Child.Scale.X, (Child.ObjectTexture.Height / 2f) * Child.Scale.Y), PhysicsRaylibConversions.Box2DToRaylib(Child.ObjectRigidbody.position) * 2f, new Vector2(Child.ObjectTexture.Width * Child.Scale.X, Child.ObjectTexture.Height * Child.Scale.Y)) == true)
                                            {
                                                SelectedObject = Child;
                                                break;
                                            }
                                        }

                                        if (MathUtils.IsPointInsideRectangle(Raylib.GetScreenToWorld2D(Mouse.MousePosition, ViewportUtils.Camera) + new Vector2((Object.ObjectTexture.Width / 2f) * Object.Scale.X, (Object.ObjectTexture.Height / 2f) * Object.Scale.Y), PhysicsRaylibConversions.Box2DToRaylib(Object.ObjectRigidbody.position) * 2f, new Vector2(Object.ObjectTexture.Width * Object.Scale.X, Object.ObjectTexture.Height * Object.Scale.Y)) == true)
                                        {
                                            SelectedObject = Object;
                                            break;
                                        }

                                        
                                    }
                                }                                
                                #endregion
                            }
                            #endregion

                            // Handle keyboard controls
                            #region Handle keyboard controls
                            if (ViewportKeyboardShortcutsEnabled == true)
                            {
                                // Delete the selected game object
                                if (Raylib.IsKeyPressed((int)KeyboardKey.Delete) && SelectedObject != null)
                                {
                                    SelectedObject.Destroy();

                                    if (SelectedObject.Parent != null)
                                    {
                                        ConsoleUtils.StatusWrite($"Removing child \"{SelectedObject.ObjectName}\" from parent \"{SelectedObject.Parent.ObjectName}\"...", ConsoleUtils.StatusTypes.DEBUG);
                                        SelectedObject.Parent.Children.RemoveAt(SelectedObject.Parent.Children.IndexOf(SelectedObject));
                                    }
                                    else
                                    {
                                        ConsoleUtils.StatusWrite($"Removing child \"{SelectedObject.ObjectName}\"...", ConsoleUtils.StatusTypes.DEBUG);
                                        GameObjectsInScene.RemoveAt(GameObjectsInScene.IndexOf(SelectedObject));
                                    }

                                    SelectedObject = null;
                                }

                                // Focus on an object
                                if (Raylib.IsKeyPressed((int)KeyboardKey.F) && SelectedObject != null)
                                {
                                    ViewportUtils.Camera.Target = PhysicsRaylibConversions.Box2DToRaylib(SelectedObject.ObjectRigidbody.position) * 0.5f;
                                }

                                // Compile and run a debug build
                                if (Raylib.IsKeyPressed((int)KeyboardKey.F5))
                                {
                                    ImGui.Begin("Compiling project...");
                                    ImGui.End();
                                    
                                    try
                                    {
                                        RuntimeBuilder.RunTestBuild();
                                    }
                                    catch (Exception EX)
                                    {
                                        ConsoleUtils.StatusWrite($"Error while compiling project: {EX.Message}", ConsoleUtils.StatusTypes.ERROR);
                                    }
                                }

                                // Add a new game object
                                if (Raylib.IsKeyDown((int)KeyboardKey.LeftControl) && Raylib.IsKeyPressed((int)KeyboardKey.N) && ViewportKeyboardShortcutsEnabled == true)
                                {
                                    SelectedObject = GameObjectUtils.CreateNewGameObject(SelectedObject, ViewportUtils.Camera.Target, true);
                                }

                                // Editor actions
                                if (Keyboard.IsKeyDown(KeyboardKey.LeftControl) == true || Keyboard.IsKeyDown(KeyboardKey.RightControl) == true)
                                {
                                    // Open a project (Shift = open scene)
                                    if (Keyboard.IsKeyPressed(KeyboardKey.O) == true)
                                    {
                                        if (Keyboard.IsKeyDown(KeyboardKey.LeftShift) == true || Keyboard.IsKeyDown(KeyboardKey.RightShift) == true)
                                        {
                                            EditorActions.SelectSceneFromDisk(GameObjectsInScene, ScenesInProject);
                                        }
                                        else
                                        {
                                            EditorActions.SelectProjectFromDisk(ref GameObjectsInScene, ref ScenesInProject);
                                        }
                                    }

                                    // Save a project (Shift = save scene)
                                    if (Keyboard.IsKeyPressed(KeyboardKey.S) == true)
                                    {
                                        if (Keyboard.IsKeyDown(KeyboardKey.LeftShift) == true || Keyboard.IsKeyDown(KeyboardKey.RightShift) == true)
                                        {
                                            EditorActions.SaveSceneToDisk(CurrentScene, GameObjectsInScene);
                                        }
                                        else
                                        {
                                            EditorActions.SaveProjectToDisk(ProjectName, ProjectConfigPath, ProjectConfigFile, CurrentScene, ScenesInProject);
                                        }
                                    }

                                    // Publish a project
                                    if (Keyboard.IsKeyPressed(KeyboardKey.B) == true)
                                    {
                                        PublishWindowSetPos = true;
                                        ShowPublishWindow = true;
                                    }
                                }

                                // Wagoo wagoo cat
                                if (Keyboard.IsKeyDown(KeyboardKey.RightShift) && Keyboard.IsKeyDown(KeyboardKey.LeftAlt) && Keyboard.IsKeyPressed(KeyboardKey.F3))
                                {
                                    Messagebox.ShowMessage("Wagoo wagoo", "Wagoo wagoo?!11/?!!\nhttps://youtu.be/cdy_OPm6K1s?si=Durv-FuskaWHQgFZ", SDL2.SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION);
                                }
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        if (FPSTarget != InactiveFPS)
                        {
                            Raylib.SetTargetFPS(InactiveFPS);
                            FPSTarget = InactiveFPS;
                        }
                    }
                    #endregion

                    // End the ImGui frame
                    Runtime.GUIManager.EndFrame();

                    // If there's a background operation happening, draw a popup
                    if (BackgroundOperationRunning == true)
                    {
                        Runtime.GUIManager.NewFrame();
                        Raylib.DrawRectangle(0, 0, EditorWindowSize.Width, EditorWindowSize.Height, new Color(0, 0, 0, 128));
                        ImGui.Begin("Operation in Progress", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove);
                        ImGui.SetWindowSize(new Vector2(448, 72));
                        ImGui.SetWindowPos(new Vector2((EditorWindowSize.Width / 2f) - (ImGui.GetWindowSize().X / 2f), (EditorWindowSize.Height / 2f) - (ImGui.GetWindowSize().Y / 2f)));
                        ImGui.TextWrapped($"{BackgroundOperationText} ({string.Format("{0:0.00}%", BackgroundOperationProgress * 100f)}%)...");
                        ImGui.ProgressBar(BackgroundOperationProgress, new Vector2(ImGui.GetWindowSize().X - 16, 16), "");
                        ImGui.End();
                        Runtime.GUIManager.EndFrame();
                    }

                    // Update delta time & frame time and end ImGui & Raylib drawing
                    #region End of frame shenanigans
                    Runtime.FrameCount++;
                    Runtime.DeltaTime = Raylib.GetFrameTime();
                    Raylib.EndDrawing();

                    // Increment the frame time counter
                    FrameTime += Raylib.GetFrameTime();
                    #endregion
                }

                for(int I = 0; I < 5; I++)
                {
                    Raylib.BeginDrawing();
                    Runtime.GUIManager.NewFrame();
                    ImGui.Begin("Editor Shutting Down", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove);
                    ImGui.SetWindowSize(new Vector2(384, 48));
                    ImGui.SetWindowPos(new Vector2((EditorWindowSize.Width / 2f) - (ImGui.GetWindowSize().X / 2f), (EditorWindowSize.Height / 2f) - (ImGui.GetWindowSize().Y / 2f)));
                    ImGui.TextWrapped("Please wait while the editor shuts down...");
                    ImGui.End();
                    Runtime.GUIManager.EndFrame();
                    Raylib.EndDrawing();
                }
                
                Thread.Sleep(100);

                // Close the window and perform cleanup
                ExitEditor();
            }
            catch (Exception EX)
            {
                ConsoleUtils.StatusWrite($"Runtime error: {EX.Message}\n    {EX.StackTrace}", ConsoleUtils.StatusTypes.ERROR);
                Messagebox.ShowMessage("Saturn - ERROR", $"An error occurred.\n\nDetails:\n  Error message: {EX.Message}\n  Error code: {EX.HResult}\n  Stack trace:\n    {EX.StackTrace}", SDL2.SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR);
                ExitEditor(false, EX.HResult);
            }
        }

        private void CallUpdateOnGameObjects()
        {
            foreach (GameObject Object in GameObjectsInScene)
            {
                Object.Update();
            }
        }

        private void UpdateWindowProperties()
        {
            if (Raylib.IsWindowMaximized() == true)
            {
                CurrentWindowState = "Maximized";
            }
            else
            {
                CurrentWindowState = "Not maximized";
            }

            EditorWindowSize.Width = Raylib.GetScreenWidth();
            EditorWindowSize.Height = Raylib.GetScreenHeight();
            EditorWindowPos = Raylib.GetWindowPosition();
            ViewportPosition.X = WindowSplits.Width;
            ViewportSize.Width = EditorWindowSize.Width - (WindowSplits.Width * 2);
            ViewportSize.Height = ((EditorWindowSize.Height / 3) * 2) - 32;
            SelectionOffset = ViewportUtils.Camera.Offset - (ViewportUtils.Camera.Target * ViewportUtils.Camera.Zoom);

            if (ViewportRenderTexture.Texture.Width != ViewportSize.Width || ViewportRenderTexture.Texture.Height != ViewportSize.Height)
            {
                ViewportRenderTexture = Raylib.LoadRenderTexture(ViewportSize.Width, ViewportSize.Height);
            }            
        }

        private void ExitEditor(bool WriteConfig = true, int StatusCode = 0)
        {
            // Close the FSWatcher
            ConsoleUtils.StatusWrite("Stopping FSWatcher...", ConsoleUtils.StatusTypes.DEBUG);
            ProjectFSItemImporter.Close();

            // Unload all icons
            foreach(Icon IconToUnload in Icons)
            {
                ConsoleUtils.StatusWrite($"Unloading icon \"{IconToUnload.IconName} ({IconToUnload.IconID.Handle})\"...", ConsoleUtils.StatusTypes.DEBUG);
                Raylib.UnloadTexture(IconToUnload.IconTexture);
            }

            EntryPoint.ExitEngine(StatusCode);
        }
    }
}