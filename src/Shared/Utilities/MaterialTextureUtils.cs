/* DIRECTIVES */
using System.Drawing;
using Hexa.NET.Raylib;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class MaterialTextureUtils
    {
        /* FUNCTIONS */
        public static Texture ImageToTexture(Image IMG)
        {
            return Raylib.LoadTextureFromImage(IMG);
        }

        public static Image TextureToImage(Texture Texture)
        {
            return Raylib.LoadImageFromTexture(Texture);
        }

        public static Image GenerateCheckedImage(Size ImageSize, Size CheckBoxes, Hexa.NET.Raylib.Color Color1, Hexa.NET.Raylib.Color Color2)
        {
            return Raylib.GenImageChecked(ImageSize.Width, ImageSize.Height, CheckBoxes.Width, CheckBoxes.Height, Color1, Color2);
        }

        public static Material ImageToMaterial(Image IMG)
        {
            return TextureToMaterial(ImageToTexture(IMG));
        }

        public static Material TextureToMaterial(Texture Texture)
        {
            Material NewMaterial = Raylib.LoadMaterialDefault();
            Raylib.SetMaterialTexture(ref NewMaterial, 0, Texture);

            return NewMaterial;
        }

        /*public static Material LoadMaterialsFromFile(string Filename, int MaterialCount = 1)
        {
            unsafe
            {
                return Raylib.LoadMaterials(Filename, MaterialCount);
            }
        }*/
    }
}