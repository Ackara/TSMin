using Acklann.Diffa;
using Acklann.TSBuild.MSBuild;
using FakeItEasy;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
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

		[TestMethod]
		public void Can_generate_typescript_model_with_msbuild()
		{
			// Arrange

			// Act

			// Assert
			throw new System.NotImplementedException();
		}
	}
}
