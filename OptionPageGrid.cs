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

        [Category(BehaviorCategoryName)]
        [DisplayName("Base Path")]
        [Description("Absolute path to a directory that is used as a base path. If empty, the solution directory is used.")]
        public string OptionBasePath
        {
            get;
            set;
        }

        [Category(BehaviorCategoryName)]
        [DisplayName("URL Prefix")]
        [Description("Specify URL base path. Required for Copy URL command. Example: https://github.com/vim/vim/blob/master/")]
        public string OptionPrefix
        {
            get;
            set;
        }

        [Category(BehaviorCategoryName)]
        [DisplayName("Forward slash")]
        [Description("If enabled, replace '\\' by '/' in a copied path.")]
        public bool OptionIsForwardSlash { get; set; } = true;

        public enum StorageType
        {
            Global,
            Local
        }

        [Category(GlobalCategoryName)]
        [DisplayName("Storage type")]
        [Description("Global options (same for all solutions) or local options (specific for each solution).\nLocal options are stored in .suo file.")]
        public StorageType OptionStorageType { get; set; } = StorageType.Global;

        public void AttachSettings(SolutionSettings settings)
        {
            _settings = settings;
            if (_settings != null)
            {
                OptionBasePath = _settings.BasePath;
                OptionPrefix = _settings.Prefix;
                OptionIsForwardSlash = _settings.UseForwardSlash;
            }
        }

        public SolutionSettings BuildSettings()
        {
            return new SolutionSettings()
            {
                BasePath = OptionBasePath,
                Prefix = OptionPrefix,
                UseForwardSlash = OptionIsForwardSlash
            };
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
        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e); // options: show
        }
        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e); // options: ok (2)
            if (_settings != null)
            {
                _settings.BasePath = OptionBasePath;
                _settings.Prefix = OptionPrefix;
                _settings.UseForwardSlash = OptionIsForwardSlash;
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // options: cancel (1), ok (6)
        }
        protected override void OnDeactivate(CancelEventArgs e)
        {
            base.OnDeactivate(e); // options: ok (1)
        }
        protected override void SaveSetting(PropertyDescriptor property)
        {
            base.SaveSetting(property); // options: ok (4)
        }

        #endregion
    }
}
