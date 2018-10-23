using BetterComments.Options;
using Microsoft.VisualStudio.Shell;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace BetterComments
{
   [ProvideOptionPage(typeof(OptionsGeneralPage), "Better Comments", "General", 0, 0, true)]
   [ProvideOptionPage(typeof(OptionsTokensPage), "Better Comments", "Tokens", 0, 0, true)]
   [PackageRegistration(UseManagedResourcesOnly = true)]
   [InstalledProductRegistration("#110", "#112", Vsix.Id, IconResourceID = 400)]
   [Guid(PACKAGE_GUID_STRING)]
   [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
                     Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
   public sealed class VsPackage : Package
   {
      public const string PACKAGE_GUID_STRING = "09e59564-c21a-44f8-ae2b-c2bc17facd07";
   }
}