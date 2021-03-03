using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace CopyRelativePath
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CopyCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("31ffadf9-d4ce-44e3-8931-03823256b328");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly CopyRelativePathPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CopyCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package as CopyRelativePathPackage ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CopyCommand Instance
        {
            get;
            private set;
        }

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
            Instance = new CopyCommand(package, commandService);
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
            string basePath = package.OptionBasePath;
            if (basePath.Length == 0)
                basePath = Path.GetDirectoryName(package.DTE.Solution.FullName);

            string fileName = package.DTE.ActiveDocument.FullName;
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

            Clipboard.SetText(fileName);
        }
    }
}
