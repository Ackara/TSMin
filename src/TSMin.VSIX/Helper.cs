using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Acklann.TSMin
{
    internal static class Helper
    {
        public static EnvDTE.Project ToProject(this IVsHierarchy hierarchy)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out object objProj);
            return (objProj as EnvDTE.Project);
        }
    }
}