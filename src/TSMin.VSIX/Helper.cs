using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;

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

        public static bool TryGetSelectedProject(this EnvDTE.DTE dte, out EnvDTE.Project project)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            string selectedFile = null;
            if (dte.SelectedItems != null)
                foreach (EnvDTE.SelectedItem item in dte.SelectedItems)
                {
                    if (item.Project != null)
                    {
                        selectedFile = item.Project.FullName;
                        break;
                    }
                }

            if (string.IsNullOrEmpty(selectedFile))
                foreach (string relativePath in (Array)dte.Solution.SolutionBuild.StartupProjects)
                {
                    string rootDir = Path.GetDirectoryName(dte.Solution.FullName);
                    selectedFile = Path.Combine(rootDir, relativePath);
                    break;
                }

            project = dte.Solution.FindProjectItem(selectedFile)?.ContainingProject;
            return project != null;
        }
    }
}