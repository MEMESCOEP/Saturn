/* DIRECTIVES */
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using static SDL2.SDL;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class ProjectFSItemImporter
    {
        /* VARIABLES */
        public static bool SkipUpdate = false;
        public static bool IsFinding = false;
        static FileSystemWatcher FSWatcher = new FileSystemWatcher();
        static List <ProjectDirectory> ProjectDirs;
        static List <ProjectFile> ProjectFiles;
        static List <Icon> Icons;
        static bool DisregardEventCheck = true;


        /* FUNCTIONS */
        public static void Init(string ProjectPath, List <ProjectFile> ProjectFileList, List <ProjectDirectory> ProjectDirList, List <Icon> IconList)
        {
            try
            {
                ProjectFiles = ProjectFileList;
                ProjectDirs = ProjectDirList;
                Icons = IconList;

                FindItemsInProjectDirectory(null, null);
                DisregardEventCheck = false;

                FSWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                FSWatcher.Changed += new FileSystemEventHandler(FindItemsInProjectDirectory);
                FSWatcher.Created += new FileSystemEventHandler(FindItemsInProjectDirectory);
                FSWatcher.Deleted += new FileSystemEventHandler(FindItemsInProjectDirectory);
                FSWatcher.Renamed += new RenamedEventHandler(FindItemsInProjectDirectory);
                FSWatcher.Path = ProjectPath;
                FSWatcher.IncludeSubdirectories = true;
                FSWatcher.EnableRaisingEvents = true;
            }
            catch (Exception EX)
            {
                Close();

                if (EX.Message.Contains("The configured user limit") == true)
                {
                    SDL_MessageBoxData MessageBoxData = new SDL_MessageBoxData();
                    SDL_MessageBoxButtonData YesButton = new SDL_MessageBoxButtonData();
                    SDL_MessageBoxButtonData NoButton = new SDL_MessageBoxButtonData();

                    YesButton.buttonid = 0;
                    YesButton.text = "Yes";

                    NoButton.flags = SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT | SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT;
                    NoButton.buttonid = 1;
                    NoButton.text = "No";

                    MessageBoxData.numbuttons = 2;
                    MessageBoxData.buttons = [YesButton, NoButton];
                    MessageBoxData.message = @$"{EX.Message}
(See https://github.com/dotnet/aspnetcore/issues/3475?WT.mc_id=-blog-scottha?WT.mc_id=-blog-scottha for more information)

Should you choose to continue, any changes made in the project directory (outside of the editor) will not be detected. 

Loading and/or modifying a project while the editor is not fully working could result in project data corruption, and is highly discouraged.
Are you ABSOLUTELY SURE you want to continue?";

                    MessageBoxData.title = "Saturn - WARNING";
                    MessageBoxData.flags = SDL_MessageBoxFlags.SDL_MESSAGEBOX_WARNING;

                    SDL_ShowMessageBox(ref MessageBoxData, out int PressedButton);

                    if (PressedButton != 0)
                    {
                        ConsoleUtils.StatusWrite($"FileSystemWatcher init error: {EX.Message}", ConsoleUtils.StatusTypes.ERROR);
                        EntryPoint.ExitEngine(EX.HResult);
                    }
                }
                else
                {
                    ConsoleUtils.StatusWrite($"Init error: {EX.Message}\n\tStack trace: {EX.StackTrace}", ConsoleUtils.StatusTypes.ERROR);
                    Messagebox.ShowMessage("Saturn - ERROR", $"An error occurred during initialization.\n\nDetails:\n  Error message: {EX.Message}\n  Error code: {EX.HResult}\n  Stack trace:\n    {EX.StackTrace}", SDL2.SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR);
                    EntryPoint.ExitEngine(EX.HResult);
                }
            }
        }

        public static void Close()
        {
            FSWatcher.Dispose();
        }

        public static void FindItemsInProjectDirectory(object sender, FileSystemEventArgs e)
        {
            if (SkipUpdate == true)
            {
                return;
            }

            IsFinding = true;

            if (DisregardEventCheck == true || e != null)
            {
                //ConsoleUtils.StatusWrite($"Filesystem change for path \"{e.Name}:{e.FullPath}\" detected!", ConsoleUtils.StatusTypes.DEBUG);
                ConsoleUtils.StatusWrite($"Finding files and directories in directory \"{FSWatcher.Path}\"...", ConsoleUtils.StatusTypes.DEBUG);
                ProjectFiles.Clear();
                ProjectDirs.Clear();

                foreach (var FoundDir in Directory.GetDirectories(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories))
                {
                    ConsoleUtils.StatusWrite($"Found directory \"{FoundDir}\".", ConsoleUtils.StatusTypes.DEBUG);

                    ProjectDirectory NewDir = new ProjectDirectory();
                    NewDir.DirInformation = new DirectoryInfo(FoundDir);
                    NewDir.AssociatedIcon = Icons.Where(i => i.IconName == "Folder").FirstOrDefault();
                    ProjectDirs.Add(NewDir);
                }

                foreach (var FoundFile in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories))
                {
                    ConsoleUtils.StatusWrite($"Found file \"{FoundFile}\".", ConsoleUtils.StatusTypes.DEBUG);

                    ProjectFile NewFile = new ProjectFile();
                    NewFile.FileInformation = new FileInfo(FoundFile);

                    if (Path.HasExtension(FoundFile) == false)
                    {
                        NewFile = null;
                        continue;
                    }

                    switch (Path.GetExtension(FoundFile))
                    {
                        case ".cs":
                            NewFile.AssociatedIcon = Icons.Where(i => i.IconName == "CodeFile").FirstOrDefault();
                            break;

                        case ".txt":
                            NewFile.AssociatedIcon = Icons.Where(i => i.IconName == "TextFile").FirstOrDefault();
                            break;

                        case ".obj":
                        case ".glb":
                        case ".gltf":
                            NewFile.AssociatedIcon = Icons.Where(i => i.IconName == "Model").FirstOrDefault();
                            break;

                        case ".cfg":
                            NewFile.AssociatedIcon = Icons.Where(i => i.IconName == "ConfigFile").FirstOrDefault();
                            break;

                        case ".ogg":
                        case ".wav":
                        case ".mp3":
                            NewFile.AssociatedIcon = Icons.Where(i => i.IconName == "Audio").FirstOrDefault();
                            break;

                        case ".png":
                        case ".jpg":
                        case ".bmp":
                        case ".ico":
                            // Create a icon from the image file's data
                            try
                            {
                                Icon NewIcon = new Icon();

                                NewIcon.IconTexture = MaterialTextureImporter.LoadTextureFromFile(FoundFile);
                                NewIcon.IconName = Path.GetFileNameWithoutExtension(FoundFile);

                                GCHandle TextureHandle = GCHandle.Alloc(NewIcon.IconTexture, GCHandleType.Pinned);
                                NewIcon.IconID = new ImTextureID(TextureHandle.AddrOfPinnedObject());

                                NewFile.AssociatedIcon = NewIcon;
                            }
                            catch (Exception EX)
                            {
                                ConsoleUtils.StatusWrite($"Failed to create icon for file \"{FoundFile}\": {EX.Message}", ConsoleUtils.StatusTypes.ERROR);
                                NewFile.AssociatedIcon = Icons.Where(i => i.IconName == "Image").FirstOrDefault();
                            }

                            break;

                        case ".dll":
                            NewFile.AssociatedIcon = Icons.Where(i => i.IconName == "Library").FirstOrDefault();
                            break;

                        default:
                            NewFile.AssociatedIcon = Icons.Where(i => i.IconName == "Unknown").FirstOrDefault();
                            break;
                    }

                    ProjectFiles.Add(NewFile);
                }

                // Sort the file list by file extension
                ConsoleUtils.StatusWrite("Sorting file list...", ConsoleUtils.StatusTypes.DEBUG);
                ProjectFiles = ProjectFiles.OrderBy(PFile => PFile.FileInformation.Extension).ToList();

                ConsoleUtils.StatusWrite($"Finished finding files & directories (found {ProjectFiles.Count} files & {ProjectDirs.Count} dirs).", ConsoleUtils.StatusTypes.DEBUG);
            }

            IsFinding = false;
        }
    }
}