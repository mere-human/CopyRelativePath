// Copyright (c) mere-human. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using EnvDTE;
using EnvDTE80;
using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using IStream = Microsoft.VisualStudio.OLE.Interop.IStream;
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
    [InstalledProductRegistration("#110", "#112", "1.1", IconResourceID = 400)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionPageGrid), categoryName: OptionPageGrid.ExtensionName,
        pageName: OptionPageGrid.PageName, 0, 0, true)]
    [ProvideProfile(typeof(OptionPageGrid), categoryName: OptionPageGrid.ExtensionName,
        objectName: "Copy Relative Path Settings", categoryResourceID: 106, objectNameResourceID: 107,
        isToolsOptionPage: true, DescriptionResourceID = 108)]
    [ProvideBindingPath]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class CopyRelativePathPackage : AsyncPackage, IVsPersistSolutionOpts
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
        public OptionPageGrid OptionPage { get => (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid)); }

        public string OptionBasePath
        {
            get => OptionPage.OptionBasePath;
        }
        public string OptionPrefix
        {
            get => OptionPage.OptionPrefix;
        }
        public bool OptionIsForwardSlash
        {
            get => OptionPage.OptionIsForwardSlash;
        }
        // TODO: save to solution settings
        public string[] OptionIncludeDirs
        {
            get => OptionPage.OptionIncludeDirs;
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
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            if (OptionPage.OptionStorageType == OptionPageGrid.StorageType.Local)
            {
                LoadSettings();
            }
            else
            {
                _settings = OptionPage.BuildSettings();
            }

            await CopyCommand.InitializeAsync(this);
            await PrefixCommand.InitializeAsync(this);

            DTE = (DTE2)await GetServiceAsync(typeof(DTE));
        }

        private const string ExtensionOptionsStreamKey = "copy_rel_path";

        private SolutionSettings _settings;

        public bool LoadSettings()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solutionPersistence = GetGlobalService(typeof(SVsSolutionPersistence)) as IVsSolutionPersistence;
            return solutionPersistence.LoadPackageUserOpts(this, ExtensionOptionsStreamKey) == VSConstants.S_OK;
        }

        public bool PersistSettings()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var solutionPersistence = GetGlobalService(typeof(SVsSolutionPersistence)) as IVsSolutionPersistence;
            return solutionPersistence.SavePackageUserOpts(this, ExtensionOptionsStreamKey) == VSConstants.S_OK;
        }

        #endregion

        #region IVsPersistSolutionOpts

        // Called by the shell when a solution is opened and the SUO file is read.
        public int LoadUserOptions(IVsSolutionPersistence pPersistence, uint grfLoadOpts)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return pPersistence.LoadPackageUserOpts(this, ExtensionOptionsStreamKey);
        }

        // Called by the shell if the `key` section declared in LoadUserOptions() as 
        // being written by this package has been found in the suo file
        public int ReadUserOptions(IStream pOptionsStream, string key)
        {
            _settings = null;

            try
            {
                using (var stream = new DataStreamFromComStream(pOptionsStream))
                {
                    var serializer = new BinaryFormatter();
                    var obj = serializer.Deserialize(stream);
                    if (obj != null)
                    {
                        _settings = obj as SolutionSettings;
                    }
                }
            }
            catch
            {
            }

            if (OptionPage.OptionStorageType == OptionPageGrid.StorageType.Local)
                OptionPage.AttachSettings(_settings);

            return _settings == null ? VSConstants.E_FAIL : VSConstants.S_OK;
        }

        // Called by the shell when the SUO file is saved when a solution is closed.
        // The provider calls the shell back to let it know which options keys it will use in the suo file.
        public int SaveUserOptions(IVsSolutionPersistence pPersistence)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return pPersistence.SavePackageUserOpts(this, ExtensionOptionsStreamKey);
        }

        // Called by the shell to let the package write user options under the specified key.
        public int WriteUserOptions(IStream pOptionsStream, string key)
        {
            try
            {
                using (var stream = new DataStreamFromComStream(pOptionsStream))
                {
                    var serializer = new BinaryFormatter();
                    serializer.Serialize(stream, _settings);
                }
            }
            catch
            {
            }

            _settings = null;
            if (OptionPage.OptionStorageType == OptionPageGrid.StorageType.Local)
                OptionPage.AttachSettings(null);

            return VSConstants.S_OK;
        }

        #endregion IVsPersistSolutionOpts
    }
}
