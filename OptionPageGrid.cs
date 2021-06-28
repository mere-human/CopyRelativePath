// Copyright (c) mere-human. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel;

namespace CopyRelativePath
{
    public class OptionPageGrid : DialogPage
    {
        public const string ExtensionName = "Copy Relative Path Extension";
        public const string BehaviorCategoryName = "Behavior";
        public const string GlobalCategoryName = "Global";
        public const string PageName = "General";

        [NonSerialized]
        private SolutionSettings _settings;

        #region Option Properties

        [Category(BehaviorCategoryName)]
        [DisplayName("Base directory")]
        [Description("Absolute path to a directory that is used to resolve a relative path.\nIf empty, the solution directory is used.")]
        public string OptionBasePath { get; set; }

        [Category("URL")]
        [DisplayName("Base path")]
        [Description("URL path that is prepended to the relative path. Required for Copy URL command.\nExample: https://github.com/vim/vim/blob/master/")]
        public string OptionPrefix { get; set; }

        [Category("URL")]
        [DisplayName("Line link suffix")]
        [Description("URL fragment that is used to refer to a specific line.\nExample result for #L: https://github.com/vim/vim/blob/master/Makefile#L100")]
        public string OptionLineSuffix { get; set; } = "#L";

        [Category(BehaviorCategoryName)]
        [DisplayName("Use forward slash")]
        [Description("If true, replace '\\' by '/' in a copied path.")]
        public bool OptionIsForwardSlash { get; set; } = true;

        [Category(BehaviorCategoryName)]
        [DisplayName("Include directories")]
        [Description("List of paths to remove from the copied file path. Used for Copy Include command to paste to C/C++ #include directive.")]
        public string[] OptionIncludeDirs { get; set; }

        public enum StorageType
        {
            Global,
            Local
        }

        [Category(GlobalCategoryName)]
        [DisplayName("Storage type")]
        [Description("Global options (same for all solutions) or local options (specific for each solution).\nLocal options are stored in a .suo file.")]
        public StorageType OptionStorageType
        {
            get => _storageType;
            set
            {
                if (_storageType != value && value == StorageType.Local)
                    AssignFromLocal();
                _storageType = value;
            }
        }
        private StorageType _storageType = StorageType.Global;

        #endregion

        public void OnSettingsLoaded(SolutionSettings settings)
        {
            _settings = settings;
            if (OptionStorageType == StorageType.Local)
                AssignFromLocal();
        }

        public void OnSetttingsSaved()
        {
            _settings = null;
        }

        private void AssignFromLocal()
        {
            if (_settings != null)
            {
                OptionBasePath = _settings.BasePath;
                OptionPrefix = _settings.Prefix;
                OptionIsForwardSlash = _settings.UseForwardSlash;
                OptionIncludeDirs = _settings.IncludeDirs;
                OptionLineSuffix = _settings.LineSuffix;
            }
        }

        private void AssignToLocal(SolutionSettings settings)
        {
            if (settings != null)
            {
                settings.BasePath = OptionBasePath;
                settings.Prefix = OptionPrefix;
                settings.UseForwardSlash = OptionIsForwardSlash;
                settings.IncludeDirs = OptionIncludeDirs;
                settings.LineSuffix = OptionLineSuffix;
            }
        }

        public SolutionSettings LocalSettingsFromGlobal()
        {
            var inst = new SolutionSettings();
            AssignToLocal(inst);
            return inst;
        }

        #region DialogPage methods
        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage(); // init package (1), cancel (2), close solution (1)
        }
        public override void LoadSettingsFromXml(IVsSettingsReader reader)
        {
            base.LoadSettingsFromXml(reader);
        }
        public override void ResetSettings()
        {
            base.ResetSettings();
        }
        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();// options: ok (3)
        }
        public override void SaveSettingsToXml(IVsSettingsWriter writer)
        {
            base.SaveSettingsToXml(writer); // close solution (3)
        }
        //
        // Summary:
        //     Returns whether a given value from a property on the AutomationObject is local
        //     to this machine (vs. being roamable to other machines)
        protected override bool IsPropertyValueMachineLocal(PropertyDescriptor property, object value, string storePath)
        {
            return base.IsPropertyValueMachineLocal(property, value, storePath); // options: ok (5)
        }
        protected override void LoadSettingFromStorage(PropertyDescriptor prop)
        {
            base.LoadSettingFromStorage(prop); // init package (2), cancel (3), ok (7), close solution (2)
        }

        // Options: Dialog is shown.
        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
        }

        // Options: OK pressed (2).
        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
            if (OptionStorageType == StorageType.Local)
                AssignToLocal(_settings);
        }
        // Options: Cancel (1) or OK (6) pressed.
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
        // Options: OK pressed (1)
        protected override void OnDeactivate(CancelEventArgs e)
        {
            base.OnDeactivate(e);
        }
        // Options: OK pressed (4)
        protected override void SaveSetting(PropertyDescriptor property)
        {
            base.SaveSetting(property);
        }

        #endregion
    }
}
