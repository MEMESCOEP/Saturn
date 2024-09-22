/* DIRECTIVES */
using Hexa.NET.Raylib;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class ThreadedResource
    {
        /* VARIABLES */
        public ThreadedResourceImporter.DataTypes ResourceType = ThreadedResourceImporter.DataTypes.NONE;
        public object ResourceData;
        public string ResourcePath;
        public bool IsResourceProcessed = false;
        public bool IsResourceReady = false;
    }
}