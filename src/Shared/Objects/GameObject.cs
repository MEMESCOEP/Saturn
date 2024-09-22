/* DIRECTIVES */
using System.Numerics;
using Newtonsoft.Json;
using Box2D.NET.Bindings;
using Hexa.NET.Raylib;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class GameObject
    {
        /* VARIABLES */
        public List<GameObject> Children = new List<GameObject>();

        [JsonIgnore]
        public GameObject Parent;
        public Vector2 SerializedRigidbodyPosition;
        public Vector2 Scale = new Vector2(1, 1);

        [JsonIgnore, JsonProperty(Required = Required.AllowNull)]
        public Texture ObjectTexture;

        [JsonIgnore, JsonProperty(Required = Required.AllowNull)]
        public B2.BodyDef ObjectRigidbody;

        [JsonIgnore, JsonProperty(Required = Required.AllowNull)]
        public ThreadedResource THResource;

        public B2.BodyId ObjectRigidbodyID;
        public B2.Rot SerializedRigidbodyRotation;

        public string ObjectTexturePath = "None";
        public string ObjectName = "New GameObject";
        public bool IsVisible = true;
        public bool IsStatic = false;


        /* FUNCTIONS */
        public void Update()
        {
            SerializedRigidbodyPosition = PhysicsRaylibConversions.Box2DToRaylib(ObjectRigidbody.position);
            SerializedRigidbodyRotation = ObjectRigidbody.rotation;

            if (THResource != null && THResource.IsResourceProcessed == true && THResource.ResourceData != null)
            {
                ObjectTexture = (Texture)THResource.ResourceData;
                THResource = null;
            }

            unsafe
            {
                ObjectRigidbody.fixedRotation = Convert.ToByte(IsStatic);
            }

            if (IsStatic == true)
            {
                ObjectRigidbody.linearVelocity = new B2.Vec2 {x = 0, y = 0};
                ObjectRigidbody.angularVelocity = 0f;
            }

            foreach (GameObject Child in Children)
            {
                Child.Update();
            }
        }

        public void Destroy()
        {
            ConsoleUtils.StatusWrite($"Destroying object \"{ObjectName}\"...", ConsoleUtils.StatusTypes.DEBUG);
            ConsoleUtils.StatusWrite($"Unloading object \"{ObjectName}\" texture...", ConsoleUtils.StatusTypes.DEBUG);
            Raylib.UnloadTexture(ObjectTexture);

            ConsoleUtils.StatusWrite($"Removing object \"{ObjectName}\" rigidbody from physics world...", ConsoleUtils.StatusTypes.DEBUG);
            
            foreach (GameObject Child in Children)
            {
                ConsoleUtils.StatusWrite($"Destroying child \"{ObjectName}\"...", ConsoleUtils.StatusTypes.DEBUG);
                Child.Destroy();
            }
        }
    }
}