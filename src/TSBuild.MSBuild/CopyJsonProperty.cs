using Microsoft.Build.Framework;
using System.IO;

namespace Acklann.TSBuild.MSBuild
{
    public class CopyJsonProperty : ITask
    {
        [Required]
        public ITaskItem SourceFile { get; set; }

        [Required]
        public ITaskItem DestinationFile { get; set; }

        [Required]
        public string JPath { get; set; }

        public bool Execute()
        {
            string src = SourceFile.GetMetadata("FullPath");
            string dest = DestinationFile.GetMetadata("FullPath");

            string[] paths = JPath.Split(';', ',');

            for (int i = 0; i < paths.Length; i++)
            {
                Json.CopyJsonProperty(src, dest, paths[i]);
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs($"Copied '{JPath}' property to '{Path.GetFileName(dest)}'", null, nameof(CopyJsonProperty), MessageImportance.Normal));
            }

            return true;
        }

        #region ITask

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        #endregion ITask
    }
}