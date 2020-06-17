using Acklann.Diffa;
using Acklann.TSBuild.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Acklann.TSBuild.Tests
{
	[TestClass]
	public class CompilationTest
	{
		[ClassInitialize]
		public static void Initialize(TestContext _)
		{
			if (Directory.Exists(NodeJS.InstallationDirectory))
				foreach (var item in Directory.EnumerateFiles(NodeJS.InstallationDirectory, "*.js"))
				{
					File.Delete(item);
				}

			NodeJS.Install();
		}

		[DataTestMethod]
		[DynamicData(nameof(GetCompilierOptions), DynamicDataSourceType.Method)]
		public void Can_compile_ts_files(string label, int expectedFiles, string configFile)
		{
			// Arrange
			var cwd = Path.Combine(AppContext.BaseDirectory, "generated", label);
			if (Directory.Exists(cwd)) Directory.Delete(cwd, recursive: true);
			Directory.CreateDirectory(cwd);
			Helper.CopyFolder(Sample.ProjectFolder, cwd);

			// Act
			var result = Compiler.Run(new CompilerOptions(Path.Combine(cwd, configFile)));
			var totalFiles = (from x in Directory.EnumerateFiles(cwd, "*.js*", SearchOption.AllDirectories)
							  where Path.GetExtension(x) != ".json"
							  select x).Count();

			var builder = new StringBuilder();
			var separator = string.Concat(Enumerable.Repeat('=', 50));
			foreach (var item in result.GeneratedFiles.OrderBy(x => Path.GetFileName(x)))
			{
				builder.AppendLine($"== {label} ({Path.GetFileName(item)})")
					   .AppendLine(separator)
					   .AppendLine(File.ReadAllText(item))
					   .AppendLine()
					   .AppendLine();
			}

			// Assert
			result.Success.ShouldBeTrue();
			result.SourceFiles.ShouldNotBeEmpty();

			totalFiles.ShouldBe(expectedFiles);
			result.GeneratedFiles.Length.ShouldBe(expectedFiles);

			Diff.Approve(builder, ".txt", label);
		}

		[DataTestMethod]
		[DynamicData(nameof(GetInvalidFiles), DynamicDataSourceType.Method)]
		public void Can_detect_ts_errors(string sourceFile, int errorLine)
		{
			// Arrange
			var cwd = Path.Combine(Path.GetTempPath(), "tsmin-errors");
			if (!Directory.Exists(cwd)) Directory.CreateDirectory(cwd);

			//sourceFile = Sample.GetFile(sourceFile).FullName;
			var config = Path.Combine(cwd, (sourceFile + ".config"));
			File.WriteAllText(config, $"{{ \"sourceFiles\": [ {{ \"include\": [ \"{sourceFile}\" ] }} ] }}");
			File.Copy(Sample.GetFile(sourceFile).FullName, Path.Combine(cwd, sourceFile), true);

			// Act
			var result = Compiler.Run(new CompilerOptions(config, false, false));
			var error = result.Errors.FirstOrDefault();

			// Assert
			result.Success.ShouldBeTrue();
			result.Errors.ShouldNotBeEmpty();

			error.Line.ShouldBe(errorLine);
			error.Column.ShouldBeGreaterThan(0);
			error.StatusCode.ShouldBeGreaterThan(0);
			error.Message.ShouldNotBeNullOrEmpty();
		}

		#region Backing Members

		private static IEnumerable<object[]> GetCompilierOptions()
		{
			(string, int) getData(string x)
			{
				string name = Path.GetFileNameWithoutExtension(x);
				return (
					name.Substring(name.IndexOf('-') + 1),
					int.Parse(name.Substring((name.LastIndexOf('-') + 1), 1))
					);
			}

			var configuraitonFiles = (from x in Directory.EnumerateFiles(Sample.ProjectFolder, "config-*.json")
									  select Path.GetFileName(x));

			foreach (string filePath in configuraitonFiles)
			{
				var (label, expectedFileCount) = getData(filePath);
				yield return new object[] { label, expectedFileCount, filePath, };
			}
		}

		private static IEnumerable<object[]> GetInvalidFiles()
		{
			var pattern = new Regex(@"\d+");
			foreach (string file in Directory.EnumerateFiles(Sample.DirectoryName, "error-*.ts").Select(x => Path.GetFileName(x)))
			{
				Match match = pattern.Match(file);
				if (match.Success && int.TryParse(match.Value, out int lineNo))
				{
					yield return new object[] { file, lineNo };
				}
			}
		}

		#endregion Backing Members
	}
}