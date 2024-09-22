/* DIRECTIVES */
using Raylib_cs;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class AudioUtils
    {
        /* FUNCTIONS */
        public static Sound LoadWaveFromFile(string Filename)
        {
            if (File.Exists(Filename) == false)
            {
                return new Sound();
            }

            return Raylib.LoadSoundFromWave(Raylib.LoadWave(Filename));
        }

        public static Sound LoadWaveFromRAM(string FileData, byte[] DataSize)
        {
            return Raylib.LoadSoundFromWave(Raylib.LoadWaveFromMemory(FileData, DataSize));
        }

        public static Music LoadMusicFromFile(string Filename)
        {
            if (File.Exists(Filename) == false)
            {
                return new Music();
            }

            return Raylib.LoadMusicStream(Filename);
        }

        /*public static Music LoadMusicFromRAM(string Filename)
        {
            return Raylib.LoadMusicStream(Filename);
        }*/

        public static void PlaySound(Sound SoundToPlay)
        {
            Raylib.PlaySound(SoundToPlay);
        }
    }
}