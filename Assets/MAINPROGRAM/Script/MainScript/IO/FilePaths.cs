using UnityEngine;

public class FilePaths
{
    private const string Home_Directory_Symbol = "~/";

    public static readonly string root = $"{Application.dataPath}/gameData/";

    //Resources FilePath Graphics
    public static readonly string resources_graphics = "Graphics/";
    public static readonly string resources_backgroundImages = $"{resources_graphics}BG Images/";
    public static readonly string resources_backgroundVideo = $"{resources_graphics}BG Videos/";
    public static readonly string resources_blendTexture = $"{resources_graphics}Transition Effects/";

    //Resources FilePath Audio
    public static readonly string resources_audio = "Audio/";
    public static readonly string resources_sfx = $"{resources_audio}SFX/";
    public static readonly string resources_voices = $"{resources_audio}Voices/";
    public static readonly string resources_music = $"{resources_audio}Music/";
    public static readonly string resources_ambience = $"{resources_audio}Ambience/";


    public static string GetPathToResource(string defaultPath, string resourceName)
    {
        if(resourceName.StartsWith(Home_Directory_Symbol))
            return resourceName.Substring(Home_Directory_Symbol.Length);
        
        return defaultPath + resourceName;
    }
}
