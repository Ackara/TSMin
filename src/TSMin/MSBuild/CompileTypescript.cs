using Microsoft.Build.Framework;

namespace Acklann.TSMin.MSBuild
{
    public class CompileTypescript : ITask
    {
        public bool Minify { get; set; }

        public bool GenerateSourceMap { get; set; }

        public bool Execute()
        {
            return true;
        }

        #region ITask

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        #endregion ITask
    }
}