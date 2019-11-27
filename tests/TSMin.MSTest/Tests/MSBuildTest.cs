using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;
using System.Linq;

namespace Acklann.TSMin.Tests
{
    [TestClass]
    public class MSBuildTest
    {
        [TestMethod]
        public void Can_invoke_compile_ts_task()
        {
            // Arrange
            var cwd = Path.Combine(Path.GetTempPath(), "tsbuild-temp");
            if (Directory.Exists(cwd)) Directory.Delete(cwd, recursive: true);
            Directory.Move(Path.Combine(Sample.DirectoryName, "Modles"), cwd);

            var mockEngine = A.Fake<Microsoft.Build.Framework.IBuildEngine>();
            A.CallTo(() => mockEngine.ProjectFileOfTaskNode).Returns(Path.Combine(cwd, "product.proj"));

            var src = Path.Combine(cwd, Path.GetFileName(Sample.File.CarTS));
            var sourceFile = A.Fake<Microsoft.Build.Framework.ITaskItem>();
            A.CallTo(() => sourceFile.GetMetadata(MSBuild.CompileTypescript.FullPath))
                .Returns(src);

            var outFile = A.Fake<Microsoft.Build.Framework.ITaskItem>();
            A.CallTo(() => outFile.GetMetadata(MSBuild.CompileTypescript.FullPath))
                .Returns(Path.ChangeExtension(src, ".js"));

            var sut = new MSBuild.CompileTypescript
            {
                BuildEngine = mockEngine,
                OutFile = Path.ChangeExtension(src, ".js"),
                SourceFiles = new Microsoft.Build.Framework.ITaskItem[] { sourceFile },
                GenerateSourceMap = true,
                Minify = true
            };

            // Act
            var success = sut.Execute();
            var generatedFiles = Directory.EnumerateFiles(cwd).Select(x => Path.GetFileName(x)).ToArray();

            // Assert
            success.ShouldBeTrue();
            generatedFiles.ShouldNotBeEmpty();
            generatedFiles.Length.ShouldBe(4);
        }
    }
}