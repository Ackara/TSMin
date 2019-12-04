using System;
using System.IO;
using System.Linq;

namespace Acklann.TSBuild
{
	internal static partial class Sample
	{
		public const string FOLDER_NAME = "samples";

		public static string DirectoryName => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FOLDER_NAME);
		public static string ProjectFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FOLDER_NAME, "project");
		
		public static FileInfo GetFile(string fileName, string directory = null)
        {
            fileName = Path.GetFileName(fileName);
            string searchPattern = $"*{Path.GetExtension(fileName)}";

            string targetDirectory = directory?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FOLDER_NAME);
            return new DirectoryInfo(targetDirectory).EnumerateFiles(searchPattern, SearchOption.AllDirectories)
                .First(x => x.Name.Equals(fileName, StringComparison.CurrentCultureIgnoreCase));
        }

		public static FileInfo GetError5TS() => GetFile(@"error-5.ts");
		public static FileInfo GetIndexHTML() => GetFile(@"index.html");
		public static FileInfo GetProjXML() => GetFile(@"proj.xml");
		public static FileInfo GetConfigBundle1JSON() => GetFile(@"project\config-bundle-1.json");
		public static FileInfo GetConfigDefault8JSON() => GetFile(@"project\config-default-8.json");
		public static FileInfo GetConfigExpanded4JSON() => GetFile(@"project\config-expanded-4.json");
		public static FileInfo GetConfigMinBundle2JSON() => GetFile(@"project\config-min-bundle-2.json");
		public static FileInfo GetServerTS() => GetFile(@"project\domain\Server.ts");
		public static FileInfo GetToastTS() => GetFile(@"project\domain\Toast.ts");
		public static FileInfo GetCarTS() => GetFile(@"project\Models\Car.ts");
		public static FileInfo GetObservableTS() => GetFile(@"project\Models\Observable.ts");

		public struct File
		{
			public const string Error5TS = @"error-5.ts";
			public const string IndexHTML = @"index.html";
			public const string ProjXML = @"proj.xml";
			public const string ConfigBundle1JSON = @"project\config-bundle-1.json";
			public const string ConfigDefault8JSON = @"project\config-default-8.json";
			public const string ConfigExpanded4JSON = @"project\config-expanded-4.json";
			public const string ConfigMinBundle2JSON = @"project\config-min-bundle-2.json";
			public const string ServerTS = @"project\domain\Server.ts";
			public const string ToastTS = @"project\domain\Toast.ts";
			public const string CarTS = @"project\Models\Car.ts";
			public const string ObservableTS = @"project\Models\Observable.ts";
		}
	}	
}
