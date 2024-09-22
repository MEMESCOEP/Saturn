/* DIRECTIVES */
using System.Numerics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Box2D.NET.Bindings;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class SceneImporter
    {
        /* FUNCTIONS */
        public static void ImportSceneFile(string Filename, ref List<GameObject> ObjectList)
        {
            ConsoleUtils.StatusWrite($"Importing scene \"{Filename}\"...", ConsoleUtils.StatusTypes.DEBUG);

            string SceneData = File.ReadAllText(Filename);
            string SceneGameObjectData = SceneData.Split("[GAMEOBJECTS]")[1].Split("[END_GAMEOBJECTS]")[0];

            foreach (var Object in SceneGameObjectData.Split('\n'))
            {
                if (String.IsNullOrWhiteSpace(Object) || String.IsNullOrEmpty(Object))
                {
                    continue;
                }

                JObject DeserializedJSON = JObject.Parse(Object);
                GameObject NewGameObject = JsonConvert.DeserializeObject<GameObject>(Object);
                ThreadedResource NewResource = new ThreadedResource();
                NewResource.ResourceType = ThreadedResourceImporter.DataTypes.IMAGE;
                
                //NewGameObject.ObjectTexture = MaterialTextureImporter.LoadTextureFromFile(NewGameObject.ObjectTexturePath);
                ThreadedResourceImporter.LoadDataFromFile(NewGameObject.ObjectTexturePath, false, NewResource, ThreadedResourceImporter.DataTypes.IMAGE);
                AssignNewRBToGameObject(NewGameObject, null);
                NewGameObject.THResource = NewResource;
                ConsoleUtils.StatusWrite($"Loaded GameObject \"{NewGameObject.ObjectName}\".", ConsoleUtils.StatusTypes.DEBUG);

                foreach (GameObject ChildObject in NewGameObject.Children)
                {
                    //ChildObject.ObjectTexture = MaterialTextureImporter.LoadTextureFromFile(ChildObject.ObjectTexturePath);
                    ThreadedResource NewChildResource = new ThreadedResource();
                    NewChildResource.ResourceType = ThreadedResourceImporter.DataTypes.IMAGE;

                    ThreadedResourceImporter.LoadDataFromFile(ChildObject.ObjectTexturePath, false, NewChildResource, ThreadedResourceImporter.DataTypes.IMAGE);
                    ChildObject.THResource = NewChildResource;
                    AssignNewRBToGameObject(ChildObject, NewGameObject);
                    ConsoleUtils.StatusWrite($"Loaded GameObject \"{ChildObject.ObjectName}\" (child of \"{NewGameObject.ObjectName}\").", ConsoleUtils.StatusTypes.DEBUG);

                    foreach (GameObject ChildOfChild in ChildObject.Children)
                    {
                        //ChildOfChild.ObjectTexture = MaterialTextureImporter.LoadTextureFromFile(ChildOfChild.ObjectTexturePath);
                        ThreadedResource NewCoCResource = new ThreadedResource();
                        NewCoCResource.ResourceType = ThreadedResourceImporter.DataTypes.IMAGE;

                        ThreadedResourceImporter.LoadDataFromFile(ChildOfChild.ObjectTexturePath, false, NewCoCResource, ThreadedResourceImporter.DataTypes.IMAGE);
                        ChildOfChild.THResource = NewCoCResource;
                        AssignNewRBToGameObject(ChildOfChild, ChildObject);
                        ConsoleUtils.StatusWrite($"Loaded GameObject \"{ChildOfChild.ObjectName}\" (child of \"{ChildObject.ObjectName}\").", ConsoleUtils.StatusTypes.DEBUG);
                    }
                }

                ObjectList.Add(NewGameObject);
                ConsoleUtils.StatusWrite("Loaded object successfully.", ConsoleUtils.StatusTypes.DEBUG);
            }
        }

        public static void AssignNewRBToGameObject(GameObject Object, GameObject Parent)
        {
            var NewRBData = Physics.CreateNewRigidbody(Object.SerializedRigidbodyPosition, new Vector2(Object.ObjectTexture.Width, Object.ObjectTexture.Height) * Object.Scale);

            Object.ObjectRigidbody = (B2.BodyDef)NewRBData[0];
            Object.ObjectRigidbody.position = PhysicsRaylibConversions.RaylibToBox2D(Object.SerializedRigidbodyPosition);
            Object.ObjectRigidbody.rotation = Object.SerializedRigidbodyRotation;
            Object.ObjectRigidbody.isAwake = Convert.ToByte(true);
            Object.ObjectRigidbody.isEnabled = Convert.ToByte(true);
            Object.ObjectRigidbodyID = (B2.BodyId)NewRBData[1];
        }
    }
}