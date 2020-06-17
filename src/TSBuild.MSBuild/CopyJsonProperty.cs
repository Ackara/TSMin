using Microsoft.Build.Framework;
using System.IO;

namespace Acklann.TSBuild.MSBuild
{
	public class CopyJsonProperty : ITask
	{
		[Required]
		public ITaskItem SourceFile { get; set; }

		public ITaskItem DestinationFile { get; set; }

		[Required]
		public string JPath { get; set; }

		public bool Execute()
		{
			string src = SourceFile.GetMetadata("FullPath");
			string dest = (DestinationFile?.GetMetadata("FullPath") ?? Path.Combine(Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode), "appsettings.json"));

			foreach (string path in JPath.Split(new char[] { ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries))
			{
				Json.CopyJsonProperty(src, dest, path);
				BuildEngine.LogMessageEvent(new BuildMessageEventArgs($"Copied '{path}' property to '{Path.GetFileName(dest)}'", null, nameof(CopyJsonProperty), MessageImportance.Normal));
			}

			return true;
		}

		#region ITask

		public IBuildEngine BuildEngine { get; set; }
		public ITaskHost HostObject { get; set; }

		#endregion ITask
	}
}
