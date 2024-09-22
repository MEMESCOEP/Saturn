/* DIRECTIVES */
using System.Drawing;
using NativeFileDialogSharp;
using Hexa.NET.Raylib;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class MaterialTextureImporter
    {
        /* FUNCTIONS */
        public static Texture LoadTextureFromFile(string Filename)
        {
            unsafe
            {
                if (File.Exists(Filename) == false)
                {
                    if (Filename != "EmptyTexture")
                    {
                        ConsoleUtils.StatusWrite($"Texture file \"{Filename}\" doesn't exist.", ConsoleUtils.StatusTypes.ERROR);
                    }

                    return Raylib.LoadTextureFromImage(MaterialTextureUtils.GenerateCheckedImage(new Size(32, 32), new Size(4, 4), new Hexa.NET.Raylib.Color(255, 0, 255, 255), Raylib.Black));
                }

                return Raylib.LoadTexture(Filename);
            }
        }

        public static Image LoadImageFromFile(string Filename)
        {
            unsafe
            {
                if (File.Exists(Filename) == false)
                {
                    if (Filename != "EmptyTexture")
                    {
                        ConsoleUtils.StatusWrite($"Image file \"{Filename}\" doesn't exist.", ConsoleUtils.StatusTypes.ERROR);
                    }

                    return MaterialTextureUtils.GenerateCheckedImage(new Size(32, 32), new Size(4, 4), new Hexa.NET.Raylib.Color(255, 0, 255, 255), Raylib.Black);
                }

                return Raylib.LoadImage(Filename);
            }
        }

        public static Texture OpenTextureFileBrowser(Texture DefaultTexture, GameObject ObjectToUpdate = null)
        {
            DialogResult DR = Dialog.FileOpen("png");

            if (DR.IsOk == true)
            {
                if (ObjectToUpdate != null)
                {
                    ObjectToUpdate.ObjectTexturePath = DR.Path;
                }

                return MaterialTextureUtils.ImageToTexture(LoadImageFromFile(DR.Path));
            }
            
            return DefaultTexture;
        }

        public static Image OpenImageFileBrowser(Image DefaultImage, GameObject ObjectToUpdate = null)
        {
            DialogResult DR = Dialog.FileOpen("png");

            if (DR.IsOk == true)
            {
                if (File.Exists(DR.Path) == false)
                {
                    return Raylib.GenImageChecked(32, 32, 8, 8, new Hexa.NET.Raylib.Color(255, 0, 255, 255), Raylib.Black);
                }
                
                if (ObjectToUpdate != null)
                {
                    ObjectToUpdate.ObjectTexturePath = DR.Path;
                }

                return LoadImageFromFile(DR.Path);
            }
            
            return DefaultImage;
        }
    }
}
