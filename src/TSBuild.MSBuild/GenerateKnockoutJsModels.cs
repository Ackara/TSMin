using Microsoft.Build.Framework;
using System;

namespace Acklann.TSBuild.MSBuild
{
	public class GenerateKnockoutJsModels : ITask
	{
		[Required]
		public ITaskItem[] SourceFiles { get; set; }

		[Required]
		public ITaskItem DestinationFile { get; set; }

		public ITaskItem[] References { get; set; }

		public string Namespace { get; set; }

		public bool UseAbstract { get; set; }

		public bool Execute()
		{
			throw new NotImplementedException();
		}

		#region Backing Members

		public IBuildEngine BuildEngine { get; set; }

		public ITaskHost HostObject { get; set; }

		#endregion Backing Members
	}
}
