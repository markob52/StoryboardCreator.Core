using System.IO.Compression;
using System.Runtime.CompilerServices;
using MessagePack;

namespace StoryboardCreator.Core;

/// <summary>
/// An object that represents a storyboard
/// </summary>
[MessagePackObject(keyAsPropertyName: true, AllowPrivate = true )]
public partial class Storyboard
{
    /// <summary>
    /// Title of the storyboard
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Author(s) of the storyboard
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// ShotList that the Storyboard contain
    /// </summary>
    private List<Shot> ShotList { get; set; } = new List<Shot>();
    
    /// <summary>
    /// Public Readonly interface to the ShotList
    /// </summary>
    [IgnoreMember]
    public IReadOnlyList<Shot> Shots => ShotList.AsReadOnly();

    /// <summary>
    /// Image Cache controller of the storyboard
    /// </summary>
    [IgnoreMember]
    private ImageCacheController ImageCacheControl { get; set; }

    /// <summary>
    /// Path to the storyboard's cache
    /// </summary>
    [IgnoreMember]
    public string CachePath { get; set; }

    /// <summary>
    /// Adds a new Shot
    /// </summary>
    /// <param name="id">ID of the new shot(-1 if on the end)</param>
    /// <param name="shot">optional argument for the new Shot data</param>
    /// <param name="imagePath">path to Shot image</param>
    public void AddShot(int id, Shot? shot = null, string? imagePath = null)
    {
        shot ??= new Shot();
        if (id >= ShotList.Count || id == -1)
        {
            ShotList.Add(shot);
            if (imagePath != null)
                AddImageToShot(imagePath, ShotList.Count - 1);
        }
        else
        {
            ShotList.Insert(id, shot);
            if (imagePath != null)
                AddImageToShot(imagePath, id);
        }
    }

    /// <summary>
    /// Adds image to a shot
    /// </summary>
    /// <param name="imagePath">path to the image</param>
    /// <param name="id">ID of that Shot to which the image is added</param>
    public void AddImageToShot(string imagePath, int id)
    {
        ShotList[id].ImageFileName = ImageCacheControl.AddImage(imagePath);
    }

    /// <summary>
    /// Switch two shots
    /// </summary>
    /// <param name="id1">Id of the first shot</param>
    /// <param name="id2">Id of the second shot</param>
    public void SwitchShots(int id1, int id2)
    {
        var temp = ShotList[id1];
        ShotList.RemoveAt(id1);
        ShotList[id1] = ShotList[id2];
        ShotList[id2] = temp;
    }

    /// <summary>
    /// Move the shot
    /// </summary>
    /// <param name="oldId">Old id</param>
    /// <param name="newId">New id</param>
    public void MoveShot(int oldId, int newId)
    {
        var temp = ShotList[oldId];
        ShotList.RemoveAt(oldId);
        ShotList.Insert(newId, temp);
    }

    /// <summary>
    /// Removes the Shot
    /// </summary>
    /// <param name="id"></param>
    public void RemoveShot(int id)
    {
        ShotList.RemoveAt(id);
    }

    /// <summary>
    /// Get a Shot from it's id
    /// </summary>
    /// <param name="id">id of a shot</param>
    /// <returns></returns>
    public Shot GetShot(int id)
    {
        return ShotList[id];
    }


    /// <summary>
    /// Load the Storyboard from the project file(zip format)
    /// </summary>
    /// <param name="path">Path of the project file</param>
    /// <returns>Storyboard that is loaded</returns>
    public static Storyboard Load(string path)
    {
        var projPath = GetAvailableCacheDirectory();
        ZipFile.ExtractToDirectory(path, projPath);
        var coreDataPath = Path.Combine(projPath, "core.msgpack");
        var storyboard = MessagePackSerializer.Deserialize<Storyboard>(File.ReadAllBytes(coreDataPath));
        storyboard.ImageCacheControl = new ImageCacheController(Path.Combine(projPath, "Images"));
        storyboard.CachePath = projPath;
        return storyboard;
    }

    /// <summary>
    /// A method that gives the available directory for storyboard cache while its open
    /// </summary>
    /// <returns>Directory absulute path</returns>
    private static string GetAvailableCacheDirectory()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "StoryboardCreator");
        Directory.CreateDirectory(tempPath);
        if (Directory.GetFiles(tempPath).Length == 0)
        {
            return Path.Combine(tempPath, "0");
        }
        var projPath = (Directory.GetFiles(tempPath)
            .Select(s =>
                long.Parse(s.Replace(Path.GetExtension(s), "")))
            .Max() + 1).ToString();
        return Path.Combine(tempPath,projPath);
    }

    /// <summary>
    /// Saves the Storyboard to the project file
    /// </summary>
    /// <param name="path">Path where that file will be saved</param>
    public void Save(string path)
    {
        File.WriteAllBytes(Path.Combine(CachePath, "core.msgpack"), MessagePackSerializer.Serialize(this));
        if(File.Exists(path)) File.Delete(path);
        ZipFile.CreateFromDirectory(CachePath, path);
    }

    /// <summary>
    /// Deletes the Cache
    /// </summary>
    public void Close()
    {
        Directory.Delete(CachePath, true);
        if (Directory.GetFiles(Directory.GetParent(CachePath)?.ToString() ?? string.Empty).Length == 0)
            Directory.Delete(Directory.GetParent(CachePath)?.ToString() ?? string.Empty, false);
    }

    /// <summary>
    /// Creates a new empty storyboard(with initialized Cache path)
    /// </summary>
    public Storyboard()
    {
        CachePath = GetAvailableCacheDirectory();
        ImageCacheControl = new ImageCacheController(Path.Combine(CachePath, "Images"));
    }
}