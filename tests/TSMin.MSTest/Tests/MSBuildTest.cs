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

            var sourceFile = Path.Combine(cwd, Path.GetFileName(Sample.File.CarTS));

            var mockEngine = A.Fake<Microsoft.Build.Framework.IBuildEngine>();
            A.CallTo(() => mockEngine.ProjectFileOfTaskNode).Returns(Path.Combine(cwd, "product.proj"));

            var mockItem = A.Fake<Microsoft.Build.Framework.ITaskItem>();
            A.CallTo(() => mockItem.ItemSpec).Returns(sourceFile);
            A.CallTo(() => mockItem.GetMetadata(nameof(FileInfo.FullName))).Returns(sourceFile);
            A.CallTo(() => mockItem.GetMetadata(MSBuild.CompileTypescript.MetaElement)).Returns("app.js");

            var sut = new MSBuild.CompileTypescript
            {
                BuildEngine = mockEngine,
                SourceFiles = new Microsoft.Build.Framework.ITaskItem[] { mockItem },
                GenerateSourceMap = true,
                Minify = true
            };

            // Act
            var success = sut.Execute();
            var generatedFiles = Directory.EnumerateFiles(cwd).Select(x => Path.GetFileName(x)).ToArray();

            // Asser
            success.ShouldBeTrue();
            generatedFiles.ShouldNotBeEmpty();
            generatedFiles.Length.ShouldBe(4);
        }
    }
}