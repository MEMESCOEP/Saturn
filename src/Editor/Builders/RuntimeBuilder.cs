/* DIRECTIVES */
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class RuntimeBuilder
    {
        /* VARIABLES */


        /* FUNCTIONS */
        public static void Compile(string TargetRID = "CurrentPlatform", bool StartAfterCompilation = false, bool CompileForMUSL = false)
        {
            ConsoleUtils.StatusWrite("Compiling project...", ConsoleUtils.StatusTypes.DEBUG);
            Editor.BackgroundOperationProgress = 0f;
            Editor.BackgroundOperationRunning = true;
            Editor.BackgroundOperationText = "Please wait while the project is compiling";
            ProjectFSItemImporter.SkipUpdate = true;

            //SceneExporter.ExportScenesToFile($"{Editor.ProjectName}.msf");            
            Editor.BackgroundOperationProgress = 0.25f;
            ProcessStartInfo DotnetBuildProcessInfo = new ProcessStartInfo();

            DotnetBuildProcessInfo.RedirectStandardOutput = true;
            DotnetBuildProcessInfo.RedirectStandardError = true;
            DotnetBuildProcessInfo.UseShellExecute = false;
            DotnetBuildProcessInfo.CreateNoWindow = true;
            DotnetBuildProcessInfo.FileName = "dotnet";

            if (TargetRID == "CurrentPlatform")
            {
                DotnetBuildProcessInfo.Arguments = $"build \"{Editor.ProjectConfigPath}\" --ucr --self-contained --output \"Build/{RuntimeInformation.RuntimeIdentifier}\"";
            }
            else
            {
                DotnetBuildProcessInfo.Arguments = $"build \"{Editor.ProjectConfigPath}\" --runtime {TargetRID} --self-contained --output \"Build/{TargetRID}\"";
            }

            new Thread(() => {
                try
                {
                    if (Editor.ProjectName == "Empty Project")
                    {
                        throw new TargetException("You must create or open a project first.");
                    }

                    using (Process DotnetBuildProcess = Process.Start(DotnetBuildProcessInfo))
                    {
                        // Run dotnet build and check for build errors
                        while (DotnetBuildProcess.HasExited == false)
                        {
                            if (DotnetBuildProcess.StandardOutput.EndOfStream == false)
                            {
                                string Text = DotnetBuildProcess.StandardOutput.ReadLine();

                                if (Text.Contains("was restored using") == false && Text.ToLower().Contains("restore") == true)
                                {
                                    Editor.BackgroundOperationProgress += 0.25f;
                                }
                                else if (Text.Contains("Build FAILED") == true || Text.Contains(": error") == true || Text.Contains("MSBUILD : error") == true)
                                {
                                    throw new Exception(Text);
                                }
                            }

                            Thread.Sleep(1);
                        }

                        if (DotnetBuildProcess.ExitCode != 0)
                        {
                            throw new Exception($"Unknown error (exit code \"{DotnetBuildProcess.ExitCode}\").");
                        }

                        Editor.BackgroundOperationProgress = 1f;
                        Editor.BackgroundOperationRunning = false;
                        ProjectFSItemImporter.SkipUpdate = false;

                        if (StartAfterCompilation == true)
                        {
                            if (TargetRID == "CurrentPlatform")
                            {
                                Process.Start($"{Editor.ProjectConfigPath}/Build/{RuntimeInformation.RuntimeIdentifier}/{Editor.ProjectName}");
                            }
                            else
                            {
                                Process.Start($"{Editor.ProjectConfigPath}/Build/{TargetRID}{Editor.ProjectName}");
                            }
                        }
                        else
                        {
                            using Process FileOpener = new Process();

                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                FileOpener.StartInfo.FileName = "explorer";
                                FileOpener.StartInfo.Arguments = "/select,\"" + Path.Join(Editor.ProjectConfigPath, Editor.ProjectConfigPath) + "\"";
                                FileOpener.Start();
                            }
                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                            {
                                FileOpener.StartInfo.FileName = "explorer";
                                FileOpener.StartInfo.Arguments = "-R \"" + Path.Join(Editor.ProjectConfigPath, Editor.ProjectConfigPath) + "\"";
                                FileOpener.Start();
                            }
                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            {
                                FileOpener.StartInfo = new ProcessStartInfo
                                {
                                    FileName = "dbus-send",
                                    Arguments = "--print-reply --dest=org.freedesktop.FileManager1 /org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems array:string:\"" + new Uri(Path.Join(Editor.ProjectConfigPath, Editor.ProjectConfigPath)).ToString() + "\" string:\"\"",
                                    UseShellExecute = true
                                };

                                FileOpener.Start();
                            }
                        }
                    }
                }
                catch (Exception EX)
                {
                    Messagebox.ShowMessage("Saturn - ERROR", $"An error occurred during the build.\n\nDetails:\n  {EX.Message}");
                    Editor.BackgroundOperationRunning = false;
                    ProjectFSItemImporter.SkipUpdate = false;
                }
            }).Start();
        }

        public static void RunTestBuild()
        {
            Compile(StartAfterCompilation: true);
        }
    }
}