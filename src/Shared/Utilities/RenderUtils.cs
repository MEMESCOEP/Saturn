/* DIRECTIVES */
using System.Numerics;
using Hexa.NET.Raylib;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class RenderUtils
    {
        /* FUNCTIONS */
        public static void RenderObject(GameObject Object, GameObject SelectedObject = null)
        {
            if (Object.IsVisible == true)
            {
                Rectangle SrcDstRect = new Rectangle(0, 0, Object.ObjectTexture.Width, Object.ObjectTexture.Height);

                Raylib.RlPushMatrix();
                Raylib.RlTranslatef(Object.ObjectRigidbody.position.x * 2f, Object.ObjectRigidbody.position.y * 2f, 0f);
                Raylib.RlRotatef(MathUtils.Rad2Deg(MathUtils.MakeAngleFromRot(Object.ObjectRigidbody.rotation)), 0, 0, 1);
                Raylib.RlScalef(Object.Scale.X, Object.Scale.Y, 0f);
                Raylib.DrawTexturePro(Object.ObjectTexture,
                    SrcDstRect, 
                    SrcDstRect, 
                    new Vector2(Object.ObjectTexture.Width / 2f, Object.ObjectTexture.Height / 2f), 
                    0f, 
                    Raylib.White);

                if (Object == SelectedObject)
                {
                    Raylib.DrawRectangleLines(-(int)(Object.ObjectTexture.Width / 2f), -(int)(Object.ObjectTexture.Width / 2f), Object.ObjectTexture.Width, Object.ObjectTexture.Height, Raylib.Orange);
                    Raylib.DrawLine((int)(Object.ObjectTexture.Width / 2f), 0, (int)(Object.ObjectTexture.Width * 1.25f), 0, Raylib.Red);
                    Raylib.DrawLine(0, -(int)(Object.ObjectTexture.Height / 2f), 0, -(int)(Object.ObjectTexture.Height * 1.25f), Raylib.Green);
                    Raylib.DrawTriangle(new Vector2((int)(Object.ObjectTexture.Width / 2f) * 1.25f, 5f), new Vector2((int)(Object.ObjectTexture.Width / 2f) * 1.25f, -5f), new Vector2((int)(Object.ObjectTexture.Width / 2f) * 1.25f + 10f, 0f), Raylib.Red);
                }

                Raylib.RlPopMatrix();
            }
            else
            {
                float HalfSize = Object.ObjectTexture.Width / 2f;

                Raylib.RlPushMatrix();
                Raylib.RlTranslatef(Object.ObjectRigidbody.position.x, Object.ObjectRigidbody.position.y, 0f);
                Raylib.RlScalef(Object.Scale.X, Object.Scale.Y, 0f);
                Raylib.DrawLine((int)(Object.ObjectRigidbody.position.x - HalfSize), (int)(Object.ObjectRigidbody.position.y - HalfSize), (int)(Object.ObjectRigidbody.position.x + HalfSize), (int)(Object.ObjectRigidbody.position.y + HalfSize), Raylib.Red);
                Raylib.DrawLine((int)(Object.ObjectRigidbody.position.x + HalfSize), (int)(Object.ObjectRigidbody.position.y - HalfSize), (int)(Object.ObjectRigidbody.position.x - HalfSize), (int)(Object.ObjectRigidbody.position.y + HalfSize), Raylib.Red);
                Raylib.RlPopMatrix();
            }            
        }
    }
}
