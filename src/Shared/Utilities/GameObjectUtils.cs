/* DIRECTIVES */
using System.Numerics;
using Box2D.NET.Bindings;
using Hexa.NET.ImGui;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class GameObjectUtils
    {
        /* VARIABLES */
        public static List<GameObject> ObjectList;


        /* FUNCTIONS */
        public static GameObject FindDeepestChild(List<GameObject> Children)
        {
            GameObject Parent = null;

            foreach (GameObject Child in Children)
            {
                if (Child.Children.Count > 0)
                {
                    FindDeepestChild(Child.Children);
                }
            }

            return Parent;
        }

        public static void RecursiveChildList(GameObject Object)
        {
            foreach (GameObject Child in Object.Children)
            {
                ImGui.PushID(Convert.ToInt32(Object.Children.IndexOf(Child).ToString()));
                ImGui.Indent(16f);

                if (ImGui.CollapsingHeader(Child.ObjectName))
                {
                    if (Child.Children.Count > 0)
                    {
                        RecursiveChildList(Child);
                    }
                    else
                    {
                        ImGui.Indent(16f);
                        ImGui.TextUnformatted("No children.");
                        ImGui.Unindent(16f);
                    }
                }

                ImGui.PopID();
                ImGui.Unindent(16f);
            }
        }

        public static GameObject[] GetAllChildren(GameObject Object)
        {
            List<GameObject> AllChildren = new List<GameObject>();

            foreach (GameObject Child in Object.Children)
            {
                AllChildren.Add(Child);

                if (Child.Children.Count > 0)
                {
                    GetAllChildren(Child);
                }
            }

            return AllChildren.ToArray();
        }

        public static GameObject CreateNewGameObject(GameObject SelectedObject, Vector2 ObjectPosition, bool RunningInEditor = false)
        {
            GameObject NewGameObject = new GameObject();
            var NewRBData = Physics.CreateNewRigidbody(ObjectPosition, new Vector2(NewGameObject.ObjectTexture.Width, NewGameObject.ObjectTexture.Height) * NewGameObject.Scale);

            NewGameObject.ObjectRigidbody = (B2.BodyDef)NewRBData[0];
            NewGameObject.ObjectRigidbodyID = (B2.BodyId)NewRBData[1];
            NewGameObject.ObjectRigidbody.position = PhysicsRaylibConversions.RaylibToBox2D(NewGameObject.SerializedRigidbodyPosition);

            NewGameObject.ObjectTexturePath = "EmptyTexture";
            NewGameObject.ObjectTexture = MaterialTextureImporter.LoadTextureFromFile("EmptyTexture");
            NewGameObject.ObjectName = "New Object";

            if (SelectedObject != null)
            {
                SelectedObject.Children.Add(NewGameObject);
                NewGameObject.Parent = SelectedObject;
            }
            else if (RunningInEditor == true)
            {
                Editor.GameObjectsInScene.Add(NewGameObject);
            }
            else
            {
                ObjectList.Add(NewGameObject);
            }            

            return NewGameObject;
        }

        public static GameObject FindObject(string ObjectName)
        {
            foreach(GameObject Object in ObjectList)
            {
                if (Object.ObjectName == ObjectName)
                {
                    return Object;
                }

                foreach (GameObject Child in GameObjectUtils.GetAllChildren(Object))
                {
                    if (Object.ObjectName == ObjectName)
                    {
                        return Child;
                    }
                }
            }

            return null;
        }
    }
}