using MessagePack;

namespace StoryboardCreator.Core;

/// <summary>
/// An object that represents one Shot
/// </summary>
[MessagePackObject(true)]
public class Shot
{
    /// <summary>
    /// Title of a shot
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Body text of a shot
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Name of an image file stored in cache or in the shots object file
    /// </summary>
    public string? ImageFileName { get; set; }
}