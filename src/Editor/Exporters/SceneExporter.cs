/* DIRECTIVES */
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class SceneExporter
    {
        /* FUNCTIONS */
        public static void ExportScenesToFile(string Filename)
        {
            List<string> SceneFiles = new List<string>();

            string ProjectFileData = File.ReadAllText(Editor.ProjectConfigPath).Split("[SCENES]")[1].Split("[END_SCENES]")[0];

            foreach (var Scene in ProjectFileData.Split('\n'))
            {
                if (String.IsNullOrWhiteSpace(Scene) || String.IsNullOrEmpty(Scene))
                {
                    continue;
                }

                ConsoleUtils.StatusWrite($"Adding scene \"{Scene}\" to list...");
                SceneFiles.Add(Scene);
            }

            using (FileStream SceneFile = File.Open(Filename, FileMode.OpenOrCreate))
            {
                using (var writer = new BinaryWriter(SceneFile, Encoding.UTF8, false))
                {
                    foreach (string Scene in SceneFiles)
                    {
                        var SceneData = File.ReadAllBytes(Scene);
                        writer.Write($"[SCENE]{Path.GetFileName(Scene)},{SceneData.Length},");
                        //writer.Write(SceneData.Length);
                        writer.Write(SceneData);
                        writer.Write("[END_SCENE]");
                    }
                }
            }
        }

        public static void SaveSceneToFile(string SceneName, List<GameObject> GameObjects)
        {
            using (FileStream SceneFile = File.Open(SceneName, FileMode.OpenOrCreate))
            {
                using (StreamWriter SceneWriter = new StreamWriter(SceneFile))
                {
                    SceneWriter.WriteLine("[GAMEOBJECTS]");
                    
                    foreach (GameObject Object in GameObjects)
                    {
                        GameObjectUtils.GetAllChildren(Object);
                        JObject ObjectJ = JObject.FromObject(Object);
                        
                        SceneWriter.WriteLine(JsonConvert.SerializeObject(ObjectJ));                        
                    }
                    
                    SceneWriter.WriteLine("[END_GAMEOBJECTS]");
                }
            }
        }
    }
}