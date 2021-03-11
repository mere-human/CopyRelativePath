using System;

namespace CopyRelativePath
{
    /// <summary>
    /// The user settings that are persisted in suo file
    /// </summary>
    [Serializable]
    public class SolutionSettings
    {
        public string BasePath { get; set; }
        public string Prefix { get; set; }
        public bool UseForwardSlash { get; set; } = true;
    }
}
