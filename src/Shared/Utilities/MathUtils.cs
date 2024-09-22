/* DIRECTIVES */
using System.Numerics;
using Box2D.NET.Bindings;
using Raylib_cs;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class MathUtils
    {
        /* FUNCTIONS */
        public static bool IsPointInsideRectangle(Vector2 PointToCheck, Vector2 RectPos, Vector2 RectSize)
        {
            return (PointToCheck.X >= RectPos.X &&
                    PointToCheck.X <= RectPos.X + RectSize.X &&
                    PointToCheck.Y >= RectPos.Y &&
                    PointToCheck.Y <= RectPos.Y + RectSize.Y);
        }

        public static float Deg2Rad(float Degrees)
        {
            return Degrees * Raylib.DEG2RAD;
        }

        public static float Rad2Deg(float Radians)
        {
            return Radians * Raylib.RAD2DEG;
        }

        public static float UnwindAngle(float Angle)
        {
            if ( Angle < -Math.PI )
            {
                return (float)(Angle + 2.0f * Math.PI);
            }
            else if ( Angle > Math.PI )
            {
                return (float)(Angle - 2.0f * Math.PI);
            }

            return Angle;
        }

        // Returns an angle in radians
        public static float MakeAngleFromRot(B2.Rot RotationToConvert)
        {
            return MathF.Atan2(RotationToConvert.s, RotationToConvert.c);
        }

        public static B2.Rot NormalizeRot(B2.Rot RotToNormalize)
        {
            float Mag = MathF.Sqrt(RotToNormalize.s * RotToNormalize.s + RotToNormalize.c * RotToNormalize.c );
            float InverseMag = Mag > 0.0 ? 1.0f / Mag : 0.0f;

            B2.Rot NormalizedRot = new B2.Rot { s = RotToNormalize.c * InverseMag, c = RotToNormalize.s * InverseMag };

            return NormalizedRot;
        }

        public static B2.Rot MakeRotFromAngle(float Angle)
        {
            float X = UnwindAngle(Angle);
            float PiSquared = (float)(Math.PI * Math.PI);

            B2.Rot NewRotation;

            float Y = (float)(X + Math.PI);
            float YSquared = Y * Y;

            // Cosine needs angle in [-pi/2, pi/2]
            if (X < -0.5f * Math.PI)
            {
                NewRotation.c = -( PiSquared - 4.0f * YSquared ) / ( PiSquared + YSquared );
            }
            else if (X > 0.5f * Math.PI)
            {
                Y = (float)(X - Math.PI);
                YSquared = Y * Y;
                NewRotation.c = -( PiSquared - 4.0f * YSquared ) / ( PiSquared + YSquared );
            }
            else
            {
                YSquared = X * X;
                NewRotation.c = ( PiSquared - 4.0f * YSquared ) / ( PiSquared + YSquared );
            }

            // Sine needs angle in [0, pi]
            if (X < 0.0f)
            {
                Y = (float)(X + Math.PI);
                NewRotation.s = (float)(-16.0f * Y * (Math.PI - Y) / (5.0f * PiSquared - 4.0f * Y * (Math.PI - Y)));
            }
            else
            {
                NewRotation.s = (float)(16.0f * X * (Math.PI - X) / (5.0f * PiSquared - 4.0f * X * (Math.PI - X)));
            }

            NewRotation = NormalizeRot(NewRotation);
            return NewRotation;
        }
    }

    public class PhysicsRaylibConversions
    {
        /* FUNCTIONS */
        public static Vector2 Box2DToRaylib(B2.Vec2 VectorToConvert)
        {
            return new Vector2(VectorToConvert.x, VectorToConvert.y);
        }

        public static B2.Vec2 RaylibToBox2D(Vector2 VectorToConvert)
        {
            return new B2.Vec2 { x = VectorToConvert.X, y = VectorToConvert.Y };
        }
    }
}