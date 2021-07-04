// Copyright (c) mere-human. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Windows.Forms.Design;

namespace CopyRelativePath
{
    // Note: A better alternative could be CommonOpenFileDialog
    class FolderEditor : FolderNameEditor
    {
        protected override void InitializeDialog(FolderBrowser folderBrowser)
        {
            base.InitializeDialog(folderBrowser);
            folderBrowser.Description = "Base Directory for Relative Path";
        }
    }
}