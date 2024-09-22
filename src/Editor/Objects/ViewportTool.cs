/* DIRECTIVES */
using System.Numerics;
using Hexa.NET.Raylib;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class ViewportTool
    {
        /* VARIABLES */
        public Matrix4x4 ToolTransform;
        public Model ToolModel;
        public bool Hovered = false;
    }
}