// Copyright (c) mere-human. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace CopyRelativePath
{
    // TODO: rename - remove Copy
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CopyIncludeCommand : BaseCopyCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0300;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("31ffadf9-d4ce-44e3-8931-03823256b328");

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIncludeCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CopyIncludeCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            base.package = package as CopyRelativePathPackage ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CopyIncludeCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in TestCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CopyIncludeCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string filePath = GetRelPath();
            if (!string.IsNullOrEmpty(filePath))
            {
                if (package.OptionIncludeDirs != null && package.OptionIncludeDirs.Length != 0)
                {
                    foreach (var incl in package.OptionIncludeDirs)
                    {
                        if (filePath.StartsWith(incl, StringComparison.InvariantCultureIgnoreCase))
                        {
                            filePath = filePath.Remove(0, incl.Length);
                            filePath = filePath.TrimStart('/', '\\');
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(filePath))
                    Clipboard.SetText("#include \"" + filePath + "\"");
            }
        }
    }
}
