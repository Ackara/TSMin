using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;
using System.Linq;

namespace Acklann.TSBuild.Tests
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
    }
}