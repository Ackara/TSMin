using Acklann.Diffa;
using Acklann.TSBuild.CodeGeneration;
using Acklann.TSBuild.MSBuild;
using FakeItEasy;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Acklann.TSBuild.Tests
{
	[TestClass]
	public class MSBuildTest
	{
		[TestMethod]
		public void Can_compile_typescript_with_msbuild()
		{
			// Arrange
			var cwd = Path.Combine(Path.GetTempPath(), "tsbuild-temp");
			if (Directory.Exists(cwd)) Directory.Delete(cwd, recursive: true);
			Helper.CopyFolder(Sample.ProjectFolder, cwd);

			var mockEngine = A.Fake<Microsoft.Build.Framework.IBuildEngine>();
			A.CallTo(() => mockEngine.ProjectFileOfTaskNode).Returns(Path.Combine(cwd, "app.proj"));

			var sut = new MSBuild.CompileTypescript
			{
				BuildEngine = mockEngine,
				ConfigurationFile = null
			};

			// Act
			var success = sut.Execute();
			var generatedFiles = Directory.EnumerateFiles(cwd).Select(x => Path.GetFileName(x)).ToArray();

			// Assert
			success.ShouldBeTrue();
			generatedFiles.ShouldNotBeEmpty();
			generatedFiles.Length.ShouldBe(4);
		}

		[TestMethod]
		public void MyTestMethod()
		{
			var cwd = Path.Combine(Path.GetTempPath(), "tsbuild-temp");
			if (Directory.Exists(cwd)) Directory.Delete(cwd, recursive: true);
			Helper.CopyFolder(Sample.ProjectFolder, cwd);

			var mockEngine = A.Fake<Microsoft.Build.Framework.IBuildEngine>();
			A.CallTo(() => mockEngine.ProjectFileOfTaskNode).Returns(Path.Combine(cwd, @"C:\Users\abaker\Projects\Foodie\src\Foodie\Foodie.csproj"));

			var sut = new MSBuild.CompileTypescript
			{
				BuildEngine = mockEngine,
				ConfigurationFile = null
			};

			// Act
			var success = sut.Execute();
			var generatedFiles = Directory.EnumerateFiles(cwd).Select(x => Path.GetFileName(x)).ToArray();

			// Assert
			success.ShouldBeTrue();
			generatedFiles.ShouldNotBeEmpty();
			generatedFiles.Length.ShouldBe(4);
		}

		[TestMethod]
		public void Can_copy_json_property_from_one_file_to_another()
		{
			// Arrange
			var sourceFile = Sample.GetCopyPropertyJSON().FullName;
			var destinationFile = Path.Combine(Path.GetTempPath(), "copy-prop-task.json");

			string folder = Path.GetDirectoryName(destinationFile);
			if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
			File.WriteAllText(destinationFile, "{ \"name\": \"conkit\" }", Encoding.UTF8);

			var mockEngine = A.Fake<IBuildEngine>();
			A.CallTo(() => mockEngine.ProjectFileOfTaskNode).Returns(Path.Combine(Path.GetTempPath(), "demo.csproj"));

			var source = A.Fake<ITaskItem>();
			A.CallTo(() => source.GetMetadata(A<string>.Ignored)).Returns(sourceFile);

			var destination = A.Fake<ITaskItem>();
			A.CallTo(() => destination.GetMetadata(A<string>.Ignored)).Returns(destinationFile);

			var results = new StringBuilder();
			void append(string name, string dest = default) => results.AppendLine("-- " + name)
											   .AppendLine(string.Concat(Enumerable.Repeat('-', 50)))
											   .AppendLine(File.ReadAllText(dest ?? destinationFile))
											   .AppendLine();

			var sut = new CopyJsonProperty
			{
				BuildEngine = mockEngine,
				SourceFile = source,
				DestinationFile = destination
			};

			// Act
			/// Case: copy a literal value
			sut.JPath = "master.database";
			var case1 = sut.Execute();
			case1 = sut.Execute();
			append("Case 1: literal");

			/// Case: copy an object.
			sut.JPath = "preview";
			var case2 = sut.Execute();
			case2 = sut.Execute();
			append("Case 2: object");

			// Assert
			case1.ShouldBeTrue();
			case2.ShouldBeTrue();

			Diff.Approve(results);
		}

		[DataTestMethod]
		[DynamicData(nameof(GetTypescriptTestCases), DynamicDataSourceType.Method)]
		public void Can_generate_typescript_models_from_source_files(string label, string[] sourceFiles, TypescriptGeneratorSettings opt)
		{
			// Arrange
			var engine = A.Fake<IBuildEngine>();

			string outputFile = Path.Combine(Path.GetTempPath(), $"{label}.ts");
			var sut = new GenerateTypescriptModels(outputFile, sourceFiles)
			{
				BuildEngine = engine,
				HostObject = A.Fake<ITaskHost>(),

				Prefix = opt.Prefix,
				Suffix = opt.Suffix,
				Namespace = opt.Namespace,
				References = opt.References,
				AsAbstract = opt.UseAbstract
			};

			// Act
			var success = sut.Execute();

			// Assert
			success.ShouldBeTrue();
			Diff.ApproveFile(outputFile, label);
		}

		#region Backing Members

		private static IEnumerable<object[]> GetTypescriptTestCases()
		{
			var sourceFolder = Path.Combine(Sample.DirectoryName, "source-files");
			if (!Directory.Exists(sourceFolder)) throw new DirectoryNotFoundException($"Could not find directory at '{sourceFolder}'.");

			foreach (string sourceFilePath in Directory.EnumerateFiles(sourceFolder, "*.ts"))
			{
				yield return new object[]
				{
					Path.GetFileNameWithoutExtension(sourceFilePath),
					new string[] { sourceFilePath },
					new TypescriptGeneratorSettings()
				};
			}
		}

		#endregion Backing Members
	}
}
