using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace CopyRelativePath
{
    public class OptionPageGrid : DialogPage
    {
        public const string CategoryName = "Copy Relative Path Extension";
        public const string PageName = "General";

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
    }
}
