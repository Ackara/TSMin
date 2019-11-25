using Acklann.Diffa;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Acklann.TSMin.Tests
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
        public void Can_bundle_ts_files(string label, int expectedFiles, CompilerOptions options)
        {
            // Arrange
            var cwd = Path.Combine(AppContext.BaseDirectory, "generated", label);
            if (Directory.Exists(cwd)) Directory.Delete(cwd, recursive: true);
            Directory.CreateDirectory(cwd);
            options.OutputFile = Path.Combine(cwd, "app.js");

            var input = Directory.EnumerateFiles(Path.Combine(Sample.DirectoryName, "domain"), "*.ts").ToArray();

            // Act
            var result = Compiler.Compile(options, input);
            var totalFiles = Directory.GetFiles(cwd, "*").Length;

            var builder = new StringBuilder();
            var separator = string.Concat(Enumerable.Repeat('=', 50));
            foreach (var item in result.GeneratedFiles.OrderBy(x => x.Length))
            {
                builder.AppendLine($"== {Path.GetFileName(item)}")
                       .AppendLine(separator)
                       .AppendLine(File.ReadAllText(item))
                       .AppendLine()
                       .AppendLine();
            }

            // Assert
            result.Success.ShouldBeTrue();
            totalFiles.ShouldBe(expectedFiles);
            result.GeneratedFiles.Length.ShouldBe(expectedFiles);

            Diff.Approve(builder, ".txt", label);
        }

        [DataTestMethod]
        [DynamicData(nameof(GetInvalidFiles), DynamicDataSourceType.Method)]
        public void Can_detect_ts_errors(string documentPath, int errorLine)
        {
            // Arrange
            var cwd = Path.Combine(AppContext.BaseDirectory, "generated", "errors");
            if (Directory.Exists(cwd)) Directory.Delete(cwd, recursive: true);
            Directory.CreateDirectory(cwd);

            var options = new CompilerOptions
            {
                OutputFile = Path.Combine(cwd, $"app-{errorLine}.js"),
                GenerateSourceMaps = true,
                Minify = true
            };

            // Act
            var result = Compiler.Compile(options, documentPath);
            var error = result.Errors.FirstOrDefault();

            // Assert
            result.Success.ShouldBeTrue();
            result.Errors.ShouldNotBeEmpty();

            error.Line.ShouldBe(errorLine);
            error.Column.ShouldBeGreaterThan(0);
            error.StatusCode.ShouldBeGreaterThan(0);
            error.Message.ShouldNotBeNullOrEmpty();
        }

        [TestMethod]
        public void Can_find_ts_files_within_folder()
        {
            // Arrange
            var folder = Sample.DirectoryName;

            // Act
            var result = Compiler.FindFiles(folder);

            // Assert
            result.ShouldNotBeEmpty();
            result.ShouldAllBe(x => x.EndsWith(".ts"));
            result.ShouldAllBe(x => Path.GetFileName(x).StartsWith('_') == false);
        }

        // ==================== DATA ==================== //

        private static IEnumerable<object[]> GetCompilierOptions()
        {
            yield return new object[] { "js", 1, new CompilerOptions
            {
                Minify = false,
                GenerateSourceMaps = false,
            }};

            yield return new object[] { "map", 2, new CompilerOptions()
            {
                Minify = false,
                GenerateSourceMaps = true
            }};

            yield return new object[] { "js-min", 1, new CompilerOptions()
            {
                Minify = true,
                GenerateSourceMaps = false
            }};

            yield return new object[] { "js-min-map", 2, new CompilerOptions()
            {
                Minify = true,
                GenerateSourceMaps = true
            }};
        }

        private static IEnumerable<object[]> GetInvalidFiles()
        {
            var pattern = new Regex(@"\d+");
            foreach (string file in Directory.EnumerateFiles(Sample.DirectoryName, "error-*.ts"))
            {
                Match match = pattern.Match(Path.GetFileNameWithoutExtension(file));
                if (match.Success && int.TryParse(match.Value, out int lineNo))
                {
                    yield return new object[] { file, lineNo };
                }
            }
        }
    }
}