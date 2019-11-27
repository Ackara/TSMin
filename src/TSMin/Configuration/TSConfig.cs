using System.Collections.Generic;
using System.IO;

namespace Acklann.TSMin.Configuration
{
    public class TSConfig
    {
        public static IEnumerable<string> FindConfigurationFiles(string direcotoryPath)
        {
            if (!Directory.Exists(direcotoryPath)) yield break;
        }
    }
}