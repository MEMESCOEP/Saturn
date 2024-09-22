/* DIRECTIVES */
using NativeFileDialogSharp;
using Hexa.NET.Raylib;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class MaterialTextureExporter
    {
        /* FUNCTIONS */
        public static void SaveTextureToFile(Texture TextureToSave)
        {
            DialogResult DR = Dialog.FileSave();

            if (DR.IsOk == true)
            {
                Raylib.ExportImage(MaterialTextureUtils.TextureToImage(TextureToSave), DR.Path);
            }
        }

        public static void SaveImageToFile(Image IMG)
        {
            DialogResult DR = Dialog.FileSave();

            if (DR.IsOk == true)
            {
                Raylib.ExportImage(IMG, DR.Path);
            }
        }
    }
}