﻿// Copyright (c) mere-human. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace CopyRelativePath
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class URLAtLineCommand : BaseCopyCommand
    {
        public const int CommandId = 0x0400;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("31ffadf9-d4ce-44e3-8931-03823256b328");

        /// <summary>
        /// Initializes a new instance of the <see cref="URLCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private URLAtLineCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            base.package = package as ExtensionPackage ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static URLAtLineCommand Instance
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
            Instance = new URLAtLineCommand(package, commandService);
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
            ThreadHelper.ThrowIfNotOnUIThread();
            string filePath = GetURLPath();
            if (!string.IsNullOrEmpty(filePath))
            {
                if (package.DTE.ActiveDocument != null)
                {
                    var textSel = package.DTE.ActiveDocument.Object("TextSelection") as TextSelection;
                    if (textSel != null)
                    {
                        filePath += package.OptionLineSuffix.Length == 0 ? "#L" : package.OptionLineSuffix;
                        filePath += textSel.CurrentLine.ToString();
                        Clipboard.SetText(filePath);
                    }
                }
            }
        }
    }
}
