namespace DocTalk;

/// <summary>
/// Represent a Working directory of the application
/// </summary>
/// <param name="directory">Full path of the directory</param>
/// <param name="files">All files that the application are able to parse</param>
public class WorkingDirectoryDTO(string directory, IEnumerable<string> files)
{
    /// <summary>
    /// Full path of the directory
    /// </summary>
    public string Directory { get; } = directory;

    /// <summary>
    /// Collection of all files that the application are able to parse
    /// </summary>
    public IEnumerable<string> Files { get; } = files;
}
