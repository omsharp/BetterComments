using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using BetterComments.Options;
using Microsoft.VisualStudio.Shell;

namespace BetterComments
{
    [ProvideOptionPage(typeof(FontOptionsPage), "Better Comments", "Font Options", 0, 0, true)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PACKAGE_GUID_STRING)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules",
                     "SA1650:ElementDocumentationMustBeSpelledCorrectly",
                     Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class BetterCommentsPackage : Microsoft.VisualStudio.Shell.Package
    {
        public const string PACKAGE_GUID_STRING = "09e59564-c21a-44f8-ae2b-c2bc17facd07";
    }
}
