using Acklann.TSBuild.CodeGeneration;
using Acklann.TSBuild.CodeGeneration.Generators;
using Microsoft.Build.Framework;
using System;
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
			_outputFile = output;
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

		public string OutputType { get; set; }

		public bool Execute()
		{
			Enum.TryParse(OutputType, out FileType kind);

			var options = new TypescriptGeneratorSettings(Namespace, Prefix, Suffix, AsAbstract, (kind == FileType.KnockoutJs), References);
			if (_sourceFiles == null) _sourceFiles = SourceFiles.Select(x => x.GetMetadata("FullPath")).ToArray();

			BuildEngine.Debug("Generating typescript models ...");
			foreach (string filePath in _sourceFiles) BuildEngine.Debug($"src: '{filePath}'");

			byte[] data;
			switch (kind)
			{
				default:
				case FileType.Model:
					data = TypescriptGenerator.Emit(options, _sourceFiles);
					break;

				case FileType.KnockoutJs:
					data = KnockoutJsGenerator.Emit(options, _sourceFiles);
					break;

				case FileType.Declaration:
					data = DeclarationFileGenerator.Emit(options, _sourceFiles);
					break;
			}

			if (string.IsNullOrEmpty(_outputFile)) _outputFile = DestinationFile.GetMetadata("FullPath");
			string folder = Path.GetDirectoryName(_outputFile);
			if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

			File.WriteAllBytes(_outputFile, data);
			BuildEngine.Info($"Generated typescript file at '{_outputFile}'.", nameof(GenerateTypescriptModels));
			return true;
		}

		#region Backing Members

		private string[] _sourceFiles;
		private string _outputFile;

		public ITaskHost HostObject { get; set; }

		public IBuildEngine BuildEngine { get; set; }

		private enum FileType
		{
			Model,
			KnockoutJs,
			Declaration
		}

		#endregion Backing Members
	}
}
