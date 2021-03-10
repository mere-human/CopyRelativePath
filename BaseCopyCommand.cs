using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CopyRelativePath
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal class BaseCopyCommand
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        protected CopyRelativePathPackage package;

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        protected void Execute(bool appendPrefix)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string basePath = package.OptionBasePath;
            if (string.IsNullOrEmpty(basePath))
                basePath = Path.GetDirectoryName(package.DTE.Solution.FullName);

            // Get active document name.
            var activeDoc = package.DTE.ActiveDocument;
            string fileName = (activeDoc == null) ? "" : activeDoc.FullName;
            var items = package.DTE.SelectedItems;
            if (!items.MultiSelect && items.Count == 1)
            {
                var item = items.Item(1).ProjectItem;   // 1-based index
                // If selected item isn't the same as the active document, get its name.
                // This is the case for an item in the Solution Explorer.
                if (item != null && item.FileCount == 1 && (activeDoc == null || item.Document != activeDoc))
                    fileName = item.FileNames[0];
            }

            if (fileName.StartsWith(basePath))
            {
                fileName = fileName.Remove(0, basePath.Length);
            }
            else
            {
                // Trim common prefix
                string[] basePathComponents = basePath.Split(Path.DirectorySeparatorChar);
                string[] fileNameComponents = fileName.Split(Path.DirectorySeparatorChar);
                int minLen = Math.Min(basePathComponents.Length, fileNameComponents.Length);
                int i = 0;
                for (; i < minLen; ++i)
                {
                    if (fileNameComponents[i] != basePathComponents[i])
                        break;
                }
                var subPathComponents = fileNameComponents.Skip(i).ToArray();
                fileName = Path.Combine(subPathComponents);
            }
            fileName = fileName.TrimStart(Path.DirectorySeparatorChar);

            // TODO: convert to forward slash if prefix starts with URI
            // TODO: append /tree/master if missing for GitHub address
            if (appendPrefix && !string.IsNullOrEmpty(package.OptionPrefix))
                fileName = Path.Combine(package.OptionPrefix, fileName);

            if (package.OptionIsForwardSlash)
                fileName = fileName.Replace(Path.DirectorySeparatorChar, '/');

            Clipboard.SetText(fileName);
        }
    }
}
