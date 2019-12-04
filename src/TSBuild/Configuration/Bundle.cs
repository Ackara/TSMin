using Acklann.GlobN;
using System.Collections.Generic;
using System.IO;

namespace Acklann.TSBuild.Configuration
{
    public class Bundle
    {
        public Bundle()
        {
            Patterns = new List<string>();
        }

        public string OutFile { get; set; }

        public string Exclude { get; set; }

        public List<string> Patterns { get; set; }

        public IEnumerable<string> GetFiles(string CurrentDirectory)
        {
            if (!Directory.Exists(CurrentDirectory)) throw new DirectoryNotFoundException($"Could not find directory at '{CurrentDirectory}'.");

            foreach (Glob pattern in Patterns)
                foreach (string filePath in pattern.ResolvePath(CurrentDirectory))
                {
                    yield return filePath;
                }
        }
    }
}