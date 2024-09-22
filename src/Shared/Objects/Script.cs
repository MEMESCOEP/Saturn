/* DIRECTIVES */
using Newtonsoft.Json;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class Script
    {
        /* VARIABLES */
        public string ScriptName = "New Script";
        public string ScriptPath = String.Empty;

        [JsonIgnore, JsonProperty(Required = Required.AllowNull)]
        public string ScriptData = String.Empty;
    }
}