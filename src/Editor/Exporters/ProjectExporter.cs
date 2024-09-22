/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class ProjectExporter
    {
        /* FUNCTIONS */
        public static void SaveProject(string ProjectConfigFilePath, string ProjectName, string OpenScene, List<string> SceneList)
        {
            // Write the project data into the file
            List<string> NewProjectData = new List<string>();

            NewProjectData.Add("[PROJECT_INFO]");
            NewProjectData.Add(ProjectName);
            
            if (OpenScene.Contains(Path.GetDirectoryName(ProjectConfigFilePath)))
            {
                NewProjectData.Add($"INSIDE_PROJECT_DIR -> {OpenScene.Replace(Path.GetDirectoryName(ProjectConfigFilePath), "")}");
            }
            else
            {
                NewProjectData.Add(OpenScene);
            }

            NewProjectData.Add("[END_PROJECT_INFO]");
            NewProjectData.Add("\n[SCENES]");

            foreach (string Scene in SceneList)
            {
                if (Scene.Contains(Path.GetDirectoryName(ProjectConfigFilePath)))
                {
                    NewProjectData.Add($"INSIDE_PROJECT_DIR -> {Scene.Replace(Path.GetDirectoryName(ProjectConfigFilePath), "")}");
                }
                else
                {
                    NewProjectData.Add(Scene);
                }
            }

            NewProjectData.Add("[END_SCENES]");
            File.WriteAllLines(ProjectConfigFilePath, NewProjectData.ToArray());
            ConsoleUtils.StatusWrite("Successfully saved the project.");
        }
    }
}