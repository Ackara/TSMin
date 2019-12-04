using System;
using System.IO;

namespace Acklann.TSBuild.Tests
{
    public static class Helper
    {
        public static void CopyFolder(string source, string destination, string pattern = "*")
        {
            if (!Directory.Exists(source)) throw new DirectoryNotFoundException($"Could not find directory at '{source}'.");
            if (string.IsNullOrEmpty(destination)) throw new ArgumentNullException(nameof(destination));

            foreach (string srcFile in Directory.EnumerateFiles(source, pattern, SearchOption.AllDirectories))
            {
                string name = Path.GetFileName(srcFile);
                string folder = Path.GetDirectoryName(srcFile);
                folder = Path.Combine(destination, folder.Replace(source, "").Trim('\\'));
                string destFile = Path.Combine(folder, name);

                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.Copy(srcFile, destFile, true);
            }
        }
    }
}