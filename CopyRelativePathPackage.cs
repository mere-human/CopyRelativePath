using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace CopyRelativePath
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(CopyRelativePathPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionPageGrid), OptionPageGrid.CategoryName, OptionPageGrid.PageName, 0, 0, true)]
    [ProvideProfileAttribute(typeof(OptionPageGrid),
    OptionPageGrid.CategoryName, "Copy Relative Path Settings", 106, 107, isToolsOptionPage: true, DescriptionResourceID = 108)]
    public sealed class CopyRelativePathPackage : AsyncPackage
    {
        /// <summary>
        /// CopyRelativePathPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "27eb3794-7e10-41c3-91f3-4ffa1c376954";

        public DTE2 DTE
        {
            get;
            private set;
        }

        #region Options
        public string OptionBasePath
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.OptionBasePath;
            }
        }

        public string OptionPrefix
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.OptionPrefix;
            }
        }

        public bool OptionIsForwardSlash
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.OptionIsForwardSlash;
            }
        }
        #endregion

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await CopyCommand.InitializeAsync(this);

            DTE = (DTE2)await GetServiceAsync(typeof(DTE));
        }

        #endregion
    }

    public class OptionPageGrid : DialogPage
    {
        public const string CategoryName = "Copy Relative Path Extension";
        public const string PageName = "General";

        [Category(CategoryName)]
        [DisplayName("Base Path")]
        [Description("Specify a base for relative path")]
        public string OptionBasePath
        {
            get;
            set;
        }

        [Category(CategoryName)]
        [DisplayName("Prefix")]
        [Description("Specify a prefix to append before the relative path (e.g. GitHub repository address)")]
        public string OptionPrefix
        {
            get;
            set;
        }

        [Category(CategoryName)]
        [DisplayName("Use forward slash '/'")]
        [Description("Replaces '\\' by '/' in a path")]
        public bool OptionIsForwardSlash
        {
            get;
            set;
        }
    }
}
