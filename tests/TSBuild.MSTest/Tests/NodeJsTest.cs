using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;

namespace Acklann.TSBuild.Tests
{
    [TestClass]
    public class NodeJsTest
    {
        [TestMethod]
        public void Can_load_modules()
        {
            // Act
            NodeJS.Install();
            var wasInstalled = NodeJS.CheckInstallation();
            var modulesExist = Directory.Exists(NodeJS.PackageDirectory);

            // Assert
            wasInstalled.ShouldBeTrue();
            modulesExist.ShouldBeTrue();
        }
    }
}