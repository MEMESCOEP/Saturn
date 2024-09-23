/* IMPORTANT NOTE */
// Using OpenGL functions in a thread requires the OpenGL context to be shared between the rendering thread and the work thread.
// This is pretty useless because only one thread may use the context at any given time, which means threads have no point.
// Instead, I've implemented a threaded resource loader that loads file data into a buffer, and then processes this data on the main thread.


/* DIRECTIVES */
using NativeFileDialogSharp;
using Hexa.NET.ImGui;
using Hexa.NET.Raylib;
using Exception = System.Exception;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class EditorActions
    {
        /* FUNCTIONS */
        // Creating projects and scenes
        public static void CreateProject(ref string ProjectName, ref string ConfigPath, ref string ConfigFile)
        {
            DialogResult DR = Dialog.FileSave("mpct");

            if (DR.IsOk)
            {
                ConsoleUtils.StatusWrite($"Creating project \"{DR.Path}\"...");                                        
                ConfigPath = Path.GetDirectoryName(DR.Path);
                ConfigFile = DR.Path;
                ProjectName = Path.GetFileNameWithoutExtension(DR.Path);
                
                ConsoleUtils.StatusWrite($"Project path: \"{ConfigPath}\"", ConsoleUtils.StatusTypes.DEBUG);
                ConsoleUtils.StatusWrite($"Project file: \"{ConfigFile}\"", ConsoleUtils.StatusTypes.DEBUG);
                ConsoleUtils.StatusWrite($"Project name: \"{ProjectName}\"", ConsoleUtils.StatusTypes.DEBUG);
                ConsoleUtils.StatusWrite("Successfully created the project.");
            }
            else
            {
                ConsoleUtils.StatusWrite("Project creation cancelled.");
            }
        }


        // Loading projects and scenes
        public static void SelectProjectFromDisk(ref List<GameObject> ObjectList, ref List<string> ScenesInProject)
        {
            try
            {
                Editor.BackgroundOperationProgress = 0f;
                DialogResult DR = Dialog.FileOpen("mpct");

                if (DR.IsOk)
                {
                    Editor.BackgroundOperationProgress = 0.25f;
                    ConsoleUtils.StatusWrite($"Loading project \"{DR.Path}\"...");

                    string ProjectData = File.ReadAllText(DR.Path);
                    string ProjectInfo = ProjectData.Split("[PROJECT_INFO]")[1].Split("[END_PROJECT_INFO]")[0];
                    string SceneToOpen = ProjectInfo.Split('\n')[2].Replace("INSIDE_PROJECT_DIR -> ", Path.GetDirectoryName(DR.Path));
                    ScenesInProject.Clear();
                    Editor.BackgroundOperationProgress = 0.5f;

                    foreach (string Scene in ProjectData.Split("[SCENES]")[1].Split("[END_SCENES]")[0].Split('\n'))
                    {
                        if (string.IsNullOrEmpty(Scene) || string.IsNullOrWhiteSpace(Scene))
                        {
                            continue;
                        }

                        ScenesInProject.Add(Scene);
                    }
                    
                    Editor.BackgroundOperationProgress = 0.75f;
                    Editor.CurrentScene = SceneToOpen;
                    Editor.ProjectConfigPath = Path.GetDirectoryName(DR.Path);
                    Editor.ProjectConfigFile = DR.Path;
                    Editor.ProjectName = ProjectInfo.Split('\n')[1];
                    
                    ConsoleUtils.StatusWrite($"Project path: \"{Editor.ProjectConfigPath}\"", ConsoleUtils.StatusTypes.DEBUG);
                    ConsoleUtils.StatusWrite($"Scene to open: \"{SceneToOpen}\"", ConsoleUtils.StatusTypes.DEBUG);
                    ConsoleUtils.StatusWrite($"Opening scene \"{DR.Path}\"...", ConsoleUtils.StatusTypes.DEBUG);

                    foreach (GameObject Object in ObjectList)
                    {
                        Object.Destroy();
                    }

                    ObjectList.Clear();
                    SceneImporter.ImportSceneFile(SceneToOpen, ref ObjectList);
                    ConsoleUtils.StatusWrite("Successfully loaded the project.");
                    Editor.BackgroundOperationProgress = 1f;
                }
                else
                {
                    ConsoleUtils.StatusWrite("Project loading cancelled.");
                }
            }
            catch (Exception EX)
            {
                Messagebox.ShowMessage("Saturn - ERROR", $"An error occurred while opening the project.\n\nDetails:\n  Error message: {EX.Message}\n  Error code: {EX.HResult}\n  Stack trace:\n    {EX.StackTrace}");
                ConsoleUtils.StatusWrite($"Error while opening project: {EX.Message}", ConsoleUtils.StatusTypes.ERROR);
            }
        }

        public static void SelectSceneFromDisk(List<GameObject> ObjectList, List<string> ScenesInProject)
        {
            new Thread(() => {
                try
                {
                    Editor.BackgroundOperationRunning = true;
                    Editor.BackgroundOperationText = "Please wait while the scene is being loaded";
                    Editor.BackgroundOperationProgress = 0f;                    
                    DialogResult DR = Dialog.FileOpen("mscn");

                    if (DR.IsOk)
                    {
                        Editor.BackgroundOperationProgress = 0.25f;
                        ConsoleUtils.StatusWrite($"Opening scene \"{DR.Path}\"...");
                        ObjectList.Clear();
                        Editor.CurrentScene = DR.Path;
                        Editor.BackgroundOperationProgress = 0.75f;

                        SceneImporter.ImportSceneFile(DR.Path, ref ObjectList);
                        ConsoleUtils.StatusWrite("Successfully opened the scene.");
                        Editor.BackgroundOperationProgress = 1f;
                    }
                    else
                    {
                        ConsoleUtils.StatusWrite("Project loading cancelled.");
                    }
                }
                catch (Exception EX)
                {
                    Messagebox.ShowMessage("Saturn - ERROR", $"An error occurred while opening the project.\n\nDetails:\n  Error message: {EX.Message}\n  Error code: {EX.HResult}\n  Stack trace:\n    {EX.StackTrace}");
                    ConsoleUtils.StatusWrite($"Error while opening project: {EX.Message}", ConsoleUtils.StatusTypes.ERROR);
                }

                Thread.Sleep(100);
                Editor.BackgroundOperationRunning = false;
            }).Start();
        }

        // Saving projects and scenes
        public static void SaveProjectToDisk(string ProjectName, string ConfigPath, string ConfigFile, string CurrentScenePath, List<string> ScenesInProject)
        {
            new Thread(() => {
                try
                {
                    Editor.BackgroundOperationRunning = true;
                    Editor.BackgroundOperationText = "Please wait while the project is being saved";

                    if (!string.IsNullOrEmpty(ProjectName) && !string.IsNullOrWhiteSpace(ProjectName) && ProjectName != "Empty Project")
                    {
                        ProjectExporter.SaveProject(ConfigFile, ProjectName, CurrentScenePath, ScenesInProject);
                        
                        for(int LoopCount = 0; LoopCount < 5; LoopCount++)
                        {
                            Raylib.BeginDrawing();
                            ImGui.BeginPopup("Operation in Progress");
                            ImGui.TextUnformatted("Please wait while the project is being saved");
                            ImGui.EndPopup();
                            Raylib.EndDrawing();
                        }
                    }
                    else
                    {
                        DialogResult DR = Dialog.FileOpen("mpct");

                        if (DR.IsOk)
                        {
                            ConfigPath = Path.GetDirectoryName(DR.Path);
                            ConfigFile = DR.Path;
                            ProjectName = Path.GetFileNameWithoutExtension(DR.Path);

                            ProjectExporter.SaveProject(ConfigFile, ProjectName, CurrentScenePath, ScenesInProject);
                            Messagebox.ShowMessage("Saturn - INFO", "Project saved.");
                        }
                        else
                        {
                            ConsoleUtils.StatusWrite("Project saving cancelled.");
                        }
                    }
                }
                catch (Exception EX)
                {
                    Messagebox.ShowMessage("Saturn - ERROR", $"An error occurred while saving the project.\n\nDetails:\n  Error message: {EX.Message}\n  Error code: {EX.HResult}\n  Stack trace:\n    {EX.StackTrace}");
                    ConsoleUtils.StatusWrite($"Error while saving project: {EX.Message}", ConsoleUtils.StatusTypes.ERROR);
                }

                Thread.Sleep(100);
                Editor.BackgroundOperationRunning = false;
            }).Start();
        }

        public static void SaveSceneToDisk(string CurrentScenePath, List<GameObject> ObjectList)
        {
            new Thread(() => {
                try
                {
                    Editor.BackgroundOperationRunning = true;
                    Editor.BackgroundOperationText = "Please wait while the scene is being saved";
                    
                    if (CurrentScenePath != "New Scene")
                    {
                        SceneExporter.SaveSceneToFile(CurrentScenePath, ObjectList);
                    }
                    else
                    {
                        DialogResult DR = Dialog.FileSave("mscn");

                        if (DR.IsOk)
                        {
                            Editor.CurrentScene = DR.Path;

                            SceneExporter.SaveSceneToFile(CurrentScenePath, ObjectList);
                            ConsoleUtils.StatusWrite("Scene saved successfully.");
                        }
                        else
                        {
                            ConsoleUtils.StatusWrite("Project save cancelled.");
                        }
                    }
                }
                catch (Exception EX)
                {
                    Messagebox.ShowMessage("Saturn - ERROR", $"An error occurred while saving the project.\n\nDetails:\n  Error message: {EX.Message}\n  Error code: {EX.HResult}\n  Stack trace:\n    {EX.StackTrace}");
                    ConsoleUtils.StatusWrite($"Error while saving project: {EX.Message}", ConsoleUtils.StatusTypes.ERROR);
                }

                Thread.Sleep(100);
                Editor.BackgroundOperationRunning = false;
            }).Start();
        }
    }
}