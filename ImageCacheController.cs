using System.IO.Compression;

namespace StoryboardCreator.Core;

/// <summary>
/// An object that makes sure images are saved in cache properly while the project is open
/// </summary>
public class ImageCacheController
{
    /// <summary>
    /// Path of  Image Cache
    /// </summary>
    public string CachePath { get; }

    /// <summary>
    /// ID of the last image
    /// </summary>
    public long LastId { get; set; } = -1;

    /// <summary>
    /// Generate new Image Cache(if it exists keep load its content)
    /// </summary>
    /// <param name="path">Where to generate it</param>
    public ImageCacheController(string path)
    {
        CachePath = path;
        Directory.CreateDirectory(CachePath);
        var files = Directory.GetFiles(CachePath);
        if (files.Length != 0)
        {
            LastId = files.Select(s => long.Parse(Path.GetFileNameWithoutExtension(s))).Max();
        }
    }

    /// <summary>
    /// Adds an image to cache
    /// </summary>
    /// <param name="path">Where is the image located</param>
    /// <returns>filename</returns>
    public string AddImage(string path)
    {
        var fileName = (LastId + 1).ToString() + Path.GetExtension(path);
        File.Copy(path, Path.Combine(CachePath, fileName), false);
        LastId++;
        return fileName;
    }
}