using Microsoft.Build.Framework;
using System;

namespace Acklann.TSBuild.MSBuild
{
	public class GenerateTypescriptDeclarationFile : ITask
	{
		[Required]
		public ITaskItem[] SourceFiles { get; set; }

		[Required]
		public ITaskItem DestinationFile { get; set; }

		public string Namespace { get; set; }

		public bool Execute()
		{
			throw new NotImplementedException();
		}

		#region Backing Members

		public ITaskHost HostObject { get; set; }

		public IBuildEngine BuildEngine { get; set; }

		#endregion Backing Members
	}
}
