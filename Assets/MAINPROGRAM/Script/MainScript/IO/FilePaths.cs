using UnityEngine;

public class FilePaths
{
    public static readonly string root = $"{Application.dataPath}/gameData/";

    //Resources FilePath
    public static readonly string resources_graphics = "Graphics/";
    public static readonly string resources_backgroundImages = $"{resources_graphics}BG Images/";
    public static readonly string resources_backgroundVideo = $"{resources_graphics}BG Videos/";
    public static readonly string resources_blendTexture = $"{resources_graphics}Transition Effects/";
}
