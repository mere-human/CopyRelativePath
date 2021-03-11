using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel;

namespace CopyRelativePath
{
    public class OptionPageGrid : DialogPage
    {
        public const string CategoryName = "Copy Relative Path Extension";
        public const string PageName = "General";

        [NonSerialized]
        private SolutionSettings _settings;

        [Category(CategoryName)]
        [DisplayName("Base Path")]
        [Description("Specify a base for relative path. If empty, then the solution directory is used.")]
        public string OptionBasePath
        {
            get;
            set;
        }

        [Category(CategoryName)]
        [DisplayName("Prefix")]
        [Description("Specify a prefix to append before the relative path. Example: https://github.com/vim/vim/blob/master/")]
        public string OptionPrefix
        {
            get;
            set;
        }

        [Category(CategoryName)]
        [DisplayName("Use forward slash '/'")]
        [Description("Replace '\\' by '/' in a path.")]
        public bool OptionIsForwardSlash { get; set; } = true;

        public void AttachSettings(SolutionSettings settings)
        {
            _settings = settings;

            OptionBasePath = _settings.BasePath;
            OptionPrefix = _settings.Prefix;
            OptionIsForwardSlash = _settings.UseForwardSlash;
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
