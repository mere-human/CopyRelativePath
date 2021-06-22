// Copyright (c) mere-human. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
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

        protected void ExecuteCopy(bool appendPrefix)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string fileName = "";
            if (package.DTE.ActiveWindow.Type == vsWindowType.vsWindowTypeDocument)
            {
                // Document context menu.
                var activeDoc = package.DTE.ActiveDocument;
                if (activeDoc != null)
                    fileName = activeDoc.FullName;
            }
            
            if (string.IsNullOrEmpty(fileName) || package.DTE.ActiveWindow.Type == vsWindowType.vsWindowTypeSolutionExplorer)
            {
                // Solution explorer context menu.
                fileName = GetPathFromSelectedProjectItem();
                if (string.IsNullOrEmpty(fileName))
                {
                    // Probably, selected item is not part of the project.
                    fileName = GetPathFromHierarchy();
                }
            }

            // Do nothing if failed to obtain the correct file name.
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                return;

            string basePath = package.OptionBasePath;
            if (string.IsNullOrEmpty(basePath) || !Path.IsPathRooted(basePath))
                basePath = Path.GetDirectoryName(package.DTE.Solution.FullName);

            // Compare path components ignoring case (assume NTFS).
            if (fileName.StartsWith(basePath, StringComparison.CurrentCultureIgnoreCase))
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
                    if (!fileNameComponents[i].Equals(basePathComponents[i], StringComparison.CurrentCultureIgnoreCase))
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

        private string GetPathFromSelectedProjectItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var selItems = package.DTE.SelectedItems;
            if (selItems.Count > 0)
            {
                EnvDTE.SelectedItem item = selItems.Item(1);   // 1-based index
                if (item != null)
                {
                    var projItem = item.ProjectItem;
                    if (projItem != null)
                    {
                        // Selected item is a part of the project.
                        if (projItem.FileCount > 0)
                            return projItem.FileNames[0];
                    }
                }
            }
            return "";
        }

        private string GetPathFromHierarchy()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var slnExplorer = package.DTE.ToolWindows.SolutionExplorer;
            if (slnExplorer.SelectedItems is object[] hierItems && hierItems.Length > 0)
            {
                // Get all the parent items up to the solution item (excluding it).
                var hierPath = new List<string>();
                var hierItem = hierItems[0] as EnvDTE.UIHierarchyItem;
                if (hierItem != null && hierItem.IsSelected)
                {
                    while (hierItem != null)
                    {
                        var parent = hierItem.Collection.Parent;
                        if (parent.Equals(slnExplorer) || parent is EnvDTE.Solution)
                            break;
                        hierPath.Insert(0, hierItem.Name);
                        hierItem = parent as EnvDTE.UIHierarchyItem;
                    }
                }
                if (hierPath.Count != 0)
                {
                    string slnDir = Path.GetDirectoryName(package.DTE.Solution.FileName);
                    if (hierPath.Count > 1)
                    {
                        // Remove project item (which comes 1st) if there is no directory for it.
                        string filePath = Path.Combine(slnDir, hierPath[0]);
                        if (!Directory.Exists(filePath))
                            hierPath.RemoveAt(0);
                    }
                    return Path.Combine(slnDir, string.Join("\\", hierPath));
                }
            }
            return "";
        }
    }
}
