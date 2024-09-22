/* DIRECTIVES */
using System.Numerics;
using Box2D.NET.Bindings;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class Physics
    {
        /* VARIABLES */
        public static B2.WorldId PhysWorld;
        public static float UnitsPerMeter = 32.0f;
        public static float PhysicsFPS = 100f;
        public static bool KillPhysicsThread = false;
        public static bool Enable = false;
        public static int SubSteps = 10;


        /* FUNCTIONS */
        public static void Init()
        {
            unsafe
            {
                B2.WorldDef PhysWorldDef = B2.DefaultWorldDef();
                PhysWorldDef.gravity = new B2.Vec2 { x = 0f, y = 9.81f * UnitsPerMeter };

                PhysWorld = B2.CreateWorld(&PhysWorldDef);
            }            
        }

        public static object[] CreateNewRigidbody(Vector2 Position, Vector2 ShapeSize)
        {
            unsafe
            {
                B2.BodyDef NewBodyDef = B2.DefaultBodyDef();
                NewBodyDef.position = PhysicsRaylibConversions.RaylibToBox2D(Position);
                NewBodyDef.gravityScale = 1f;
                NewBodyDef.isEnabled = Convert.ToByte(true);
                NewBodyDef.isAwake = Convert.ToByte(true);
                NewBodyDef.type = B2.dynamicBody;

                B2.BodyId bodyId = B2.CreateBody(PhysWorld, &NewBodyDef);

                B2.Polygon box = B2.MakeBox(ShapeSize.X, ShapeSize.Y);
                B2.ShapeDef shapeDef = B2.DefaultShapeDef();
                shapeDef.friction = 0.6f;
                shapeDef.density = 1.0f;

                B2.ShapeId shapeId = B2.CreatePolygonShape(bodyId, &shapeDef, &box);

                return [NewBodyDef, bodyId, shapeId];
            }            
        }

        /*public static Vector2 GetBoundingBox(Fixture F)
        {
            return Vector2.Zero;
        }*/

        public static void StopPhysics()
        {
            KillPhysicsThread = true;
        }

        public static void UpdatePhysics()
        {
            if (Enable == true)
            {
                B2.WorldStep(PhysWorld, 1.0f / PhysicsFPS, SubSteps);
            }
        }
    }
}