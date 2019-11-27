using System.Collections.Generic;
using System.IO;

namespace Acklann.TSBuild.Configuration
{
    public class TSConfig
    {
        public static IEnumerable<string> FindConfigurationFiles(string direcotoryPath)
        {
            if (!Directory.Exists(direcotoryPath)) yield break;
        }
    }
}