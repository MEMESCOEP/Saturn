/* DIRECTIVES */
using System.Drawing;
using Hexa.NET.Raylib;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    public class ThreadedResourceImporter
    {
        public enum DataTypes
        {
            IMAGE,
            SOUND,
            FONT,
            NONE
        }

        /* VARIABLES */
        public static bool ModifyingResources = false;
        static List<ThreadedResource> ThreadedResources = new List<ThreadedResource>();
        

        /* FUNCTIONS */
        // This should only be called on the thread where the OpenGL context is active (usually the main window thread)!
        public static void ProcessResourcesOpenGL()
        {
            for(int ResourceIndex = ThreadedResources.Count - 1; ResourceIndex >= 0; ResourceIndex--)
            {
                // Make sure the data type and threaded resource object are defined
                if (ThreadedResources[ResourceIndex] == null)
                {
                    throw new NullReferenceException("Threaded resources cannot be null.");
                }

                // Use the specified data type to properly process the resource data and mark the resource as processed
                if (ThreadedResources[ResourceIndex].IsResourceProcessed == false)
                {
                    ConsoleUtils.StatusWrite($"Processing threaded resource with type \"{ThreadedResources[ResourceIndex].ResourceType.ToString()}\" and file path \"{ThreadedResources[ResourceIndex].ResourcePath}\" ({ResourceIndex}/{ThreadedResources.Count})...", ConsoleUtils.StatusTypes.DEBUG);
                
                    switch (ThreadedResources[ResourceIndex].ResourceType)
                    {
                        case DataTypes.NONE:
                            break;

                        case DataTypes.IMAGE:
                            ThreadedResources[ResourceIndex].ResourceData = Raylib.LoadTextureFromImage((Image)ThreadedResources[ResourceIndex].ResourceData);
                            break;

                        case DataTypes.SOUND:
                            //Raylib.LoadSoundAlias((Sound)Resource.ResourceData);
                            break;

                        case DataTypes.FONT:
                            //Raylib.LoadFont((Font)Resource.ResourceData);
                            break;

                        default:
                            throw new NotImplementedException($"Processing data of type \"{ThreadedResources[ResourceIndex].ResourceType.ToString()}\" is not supported.");
                    }

                    ThreadedResources[ResourceIndex].IsResourceProcessed = true;
                }
            }
        }

        public static unsafe void LoadDataFromFile(string FilePath, bool CreateNewResource = true, ThreadedResource ResourceToUse = null, DataTypes NewResourceType = DataTypes.NONE)
        {
            ModifyingResources = true;
            ConsoleUtils.StatusWrite($"Loading data from file \"{FilePath}\"...", ConsoleUtils.StatusTypes.DEBUG);

            new Thread(() => {
                try
                {
                    // Make sure the data type and threaded resource object are defined if a new resource is not to be created.
                    // If a new resource is to be created, use the passed in data type.
                    if (CreateNewResource == true)
                    {
                        ResourceToUse = new ThreadedResource();
                        ResourceToUse.ResourceType = NewResourceType;
                    }
                    else
                    {
                        if (ResourceToUse == null)
                        {
                            throw new NullReferenceException("Threaded resources cannot be null.");
                        }
                    }

                    // Use the specified data type to properly load resources from the disk
                    ResourceToUse.IsResourceProcessed = false;
                    ResourceToUse.IsResourceReady = false;
                    ResourceToUse.ResourcePath = FilePath;

                    switch (ResourceToUse.ResourceType)
                    {
                        case DataTypes.NONE:
                            break;

                        case DataTypes.IMAGE:
                            if (FilePath == "EmptyTexture")
                            {
                                ResourceToUse.ResourceData = MaterialTextureUtils.GenerateCheckedImage(new Size(32, 32), new Size(4, 4), new Hexa.NET.Raylib.Color(255, 0, 255, 255), Raylib.Black);
                                break;
                            }

                            ResourceToUse.ResourceData = MaterialTextureImporter.LoadImageFromFile(FilePath);
                            break;

                        case DataTypes.SOUND:
                            throw new NotImplementedException("Audio loading is not implemented yet.");

                        case DataTypes.FONT:
                            throw new NotImplementedException("Font loading is not implemented yet.");

                        default:
                            throw new NotImplementedException($"Loading data of type \"{ResourceToUse.ResourceType.ToString()}\" is not supported.");
                    }

                    ResourceToUse.IsResourceReady = true;
                    ThreadedResources.Add(ResourceToUse);
                }
                catch(Exception EX)
                {
                    ConsoleUtils.StatusWrite($"Threaded resource load error: {EX.Message}", ConsoleUtils.StatusTypes.ERROR);
                }

                ModifyingResources = false;
            }).Start();
        }

        public static void UnloadResource(ThreadedResource Resource)
        {
            try
            {
                // Make sure the data type and threaded resource object are defined
                if (Resource == null)
                {
                    throw new NullReferenceException("The threaded resource cannot be null.");
                }

                switch (Resource.ResourceType)
                {
                    case DataTypes.NONE:
                        break;

                    case DataTypes.IMAGE:
                        if (Resource.ResourceData.GetType() == typeof(Image))
                        {
                            Raylib.UnloadImage((Image)Resource.ResourceData);
                        }
                        else
                        {
                            Raylib.UnloadTexture((Texture)Resource.ResourceData);
                        }
                        
                        break;

                    case DataTypes.SOUND:
                        Raylib.UnloadSound((Sound)Resource.ResourceData);
                        break;

                    case DataTypes.FONT:
                        Raylib.UnloadFont((Font)Resource.ResourceData);
                        break;

                    default:
                        throw new NotImplementedException($"Unoading data of type \"{Resource.ResourceType.ToString()}\" is not supported.");
                }
            }
            catch(Exception EX)
            {
                ConsoleUtils.StatusWrite($"Threaded resource unload error: \"{EX.Message}\" (this may have resulted in a memory leak)", ConsoleUtils.StatusTypes.ERROR);
            }
        }

        public static void UnloadAllResources()
        {
            foreach(ThreadedResource Resource in ThreadedResources)
            {
                UnloadResource(Resource);
            }
        }
    }
}