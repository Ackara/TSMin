using Microsoft.Build.Framework;
using System.IO;
using System.Linq;
using System.Text;

namespace Acklann.TSBuild.MSBuild
{
	public class CompileTypescript : ITask
	{
		public ITaskItem ConfigurationFile { get; set; }

		public bool Minify { get; set; }

		public bool GenerateSourceMaps { get; set; }

		public bool Execute()
		{
			NodeJS.Install((msg, _, __) => { BuildEngine.Info(msg); });

			string projectFolder = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
			string configFilePath = (ConfigurationFile?.GetMetadata("FullPath") ?? Compiler.FindConfigurationFile(projectFolder));

			var options = new Configuration.CompilerOptions(
				configFilePath,
				Minify,
				GenerateSourceMaps);

			CompilerResult result = Compiler.Run(options, projectFolder);
			foreach (CompilerError err in result.Errors) Log(err);
			Log(result);

			return result.HasErrors == false;
		}

		#region Backing Members

		public IBuildEngine BuildEngine { get; set; }

		public ITaskHost HostObject { get; set; }

		private void Log(CompilerResult result)
		{
			if (result.Success == false) return;

			const int n = 75;
			string cwd = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
			string header(string title = "", char c = '-') => string.Concat(Enumerable.Repeat(c, n)).Insert(5, $" {title} ").Remove(n);
			string rel(string x) => (x == null ? null : string.Format("{0}\\{1}", Path.GetDirectoryName(x).Replace(cwd, string.Empty), Path.GetFileName(x)).Trim('\\'));

			var builder = new StringBuilder();
			builder.AppendLine(header($"Build started: {Path.GetFileName(cwd)}"));
			builder.AppendLine("  Source Files:");
			for (int i = 0; i < result.SourceFiles.Length; i++)
				builder.AppendLine($"    in: {rel(result.SourceFiles[i])}");

			builder.AppendLine();
			builder.AppendLine("  Compiled Files:");
			for (int i = 0; i < result.GeneratedFiles.Length; i++)
				builder.AppendLine($"    out: {rel(result.GeneratedFiles[i])}");

			builder.AppendLine(header($"errors:{result.Errors.Length} elapse:{result.Elapse.ToString("hh\\:mm\\:ss\\.fff")}", '='))
				   .AppendLine();

			BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
				builder.ToString(),
				null,
				nameof(CompileTypescript),
				MessageImportance.Normal
				));
		}

		private void Log(CompilerError error)
		{
			switch (error.Severity)
			{
				case ErrorSeverity.Error:
					BuildEngine.LogErrorEvent(new BuildErrorEventArgs(
						string.Empty,
						$"{error.StatusCode}",
						error.File,
						error.Line,
						error.Column,
						0, 0,
						error.Message,
						string.Empty,
						nameof(CompileTypescript)));
					break;

				case ErrorSeverity.Warning:
					BuildEngine.LogWarningEvent(new BuildWarningEventArgs(
						string.Empty,
						$"{error.StatusCode}",
						error.File,
						error.Line,
						error.Column,
						0, 0,
						error.Message,
						string.Empty,
						nameof(CompileTypescript)));
					break;
			}
		}

		#endregion Backing Members
	}
}
