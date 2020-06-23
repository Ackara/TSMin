using Acklann.TSBuild.CodeGeneration;
using Acklann.TSBuild.CodeGeneration.Generators;
using Microsoft.Build.Framework;
using System.IO;
using System.Linq;

namespace Acklann.TSBuild.MSBuild
{
	public class GenerateTypescriptModels : ITask
	{
		public GenerateTypescriptModels()
		{
		}

		public GenerateTypescriptModels(string output, params string[] sourceFiles)
		{
			_destination = output;
			_sourceFiles = sourceFiles;
		}

		[Required]
		public ITaskItem[] SourceFiles { get; set; }

		[Required]
		public ITaskItem DestinationFile { get; set; }

		public string[] References { get; set; }

		public string Namespace { get; set; }

		public string Prefix { get; set; }

		public string Suffix { get; set; }

		public bool AsAbstract { get; set; }

		public bool AsKnockoutJsModel { get; set; }

		public bool Execute()
		{
			var options = new TypescriptGeneratorSettings(Namespace, Prefix, Suffix, AsAbstract, AsKnockoutJsModel, References);
			if (_sourceFiles == null) _sourceFiles = SourceFiles.Select(x => x.GetMetadata("FullPath")).ToArray();

			BuildEngine.Info($"Generating typescript models ...");
			byte[] data = TypescriptGenerator.Emit(options, _sourceFiles);

			if (string.IsNullOrEmpty(_destination)) _destination = DestinationFile.GetMetadata("FullPath");
			string folder = Path.GetDirectoryName(_destination);
			if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

			File.WriteAllBytes(_destination, data);
			return true;
		}

		#region Backing Members

		private string[] _sourceFiles;
		private string _destination;

		public ITaskHost HostObject { get; set; }

		public IBuildEngine BuildEngine { get; set; }

		#endregion Backing Members
	}
}
