using System;
using System.IO;
using System.Linq;

namespace Acklann.TSBuild
{
	internal static partial class Sample
	{
		public const string FOLDER_NAME = "test-data";

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

		public static FileInfo GetCopyPropertyJSON() => GetFile(@"copy-property.json");
		public static FileInfo GetError5TS() => GetFile(@"error-5.ts");
		public static FileInfo GetIndexHTML() => GetFile(@"index.html");
		public static FileInfo GetProjXML() => GetFile(@"proj.xml");
		public static FileInfo GetScrapTS() => GetFile(@"scrap.ts");
		public static FileInfo GetScrap2TS() => GetFile(@"scrap2.ts");
		public static FileInfo GetConfigBundle1JSON() => GetFile(@"project\config-bundle-1.json");
		public static FileInfo GetConfigDefault8JSON() => GetFile(@"project\config-default-8.json");
		public static FileInfo GetConfigExpanded4JSON() => GetFile(@"project\config-expanded-4.json");
		public static FileInfo GetConfigMinBundle2JSON() => GetFile(@"project\config-min-bundle-2.json");
		public static FileInfo GetServerTS() => GetFile(@"project\domain\Server.ts");
		public static FileInfo GetToastTS() => GetFile(@"project\domain\Toast.ts");
		public static FileInfo GetCarTS() => GetFile(@"project\Models\Car.ts");
		public static FileInfo GetObservableTS() => GetFile(@"project\Models\Observable.ts");
		public static FileInfo GetLionCS() => GetFile(@"source-files\Lion.cs");
		public static FileInfo GetTransactionCS() => GetFile(@"source-files\Transaction.cs");
		public static FileInfo GetZooCS() => GetFile(@"source-files\Zoo.cs");
		public static FileInfo GetForm720CS() => GetFile(@"specs\case1\Form720.cs");
		public static FileInfo GetIbasicinfoCS() => GetFile(@"specs\case1\IBasicInfo.cs");
		public static FileInfo GetContact1CS() => GetFile(@"specs\case2\Contact1.cs");
		public static FileInfo GetForm100CS() => GetFile(@"specs\case2\Form100.cs");
		public static FileInfo GetAnimalCS() => GetFile(@"specs\case3\Animal.cs");
		public static FileInfo GetTigerCS() => GetFile(@"specs\case3\Tiger.cs");
		public static FileInfo GetIjobinfoCS() => GetFile(@"specs\case4\IJobInfo.cs");
		public static FileInfo GetBookCS() => GetFile(@"specs\case5\Book.cs");
		public static FileInfo GetLibraryCS() => GetFile(@"specs\case5\Library.cs");

		public struct File
		{
			public const string CopyPropertyJSON = @"copy-property.json";
			public const string Error5TS = @"error-5.ts";
			public const string IndexHTML = @"index.html";
			public const string ProjXML = @"proj.xml";
			public const string ScrapTS = @"scrap.ts";
			public const string Scrap2TS = @"scrap2.ts";
			public const string ConfigBundle1JSON = @"project\config-bundle-1.json";
			public const string ConfigDefault8JSON = @"project\config-default-8.json";
			public const string ConfigExpanded4JSON = @"project\config-expanded-4.json";
			public const string ConfigMinBundle2JSON = @"project\config-min-bundle-2.json";
			public const string ServerTS = @"project\domain\Server.ts";
			public const string ToastTS = @"project\domain\Toast.ts";
			public const string CarTS = @"project\Models\Car.ts";
			public const string ObservableTS = @"project\Models\Observable.ts";
			public const string LionCS = @"source-files\Lion.cs";
			public const string TransactionCS = @"source-files\Transaction.cs";
			public const string ZooCS = @"source-files\Zoo.cs";
			public const string Form720CS = @"specs\case1\Form720.cs";
			public const string IbasicinfoCS = @"specs\case1\IBasicInfo.cs";
			public const string Contact1CS = @"specs\case2\Contact1.cs";
			public const string Form100CS = @"specs\case2\Form100.cs";
			public const string AnimalCS = @"specs\case3\Animal.cs";
			public const string TigerCS = @"specs\case3\Tiger.cs";
			public const string IjobinfoCS = @"specs\case4\IJobInfo.cs";
			public const string BookCS = @"specs\case5\Book.cs";
			public const string LibraryCS = @"specs\case5\Library.cs";
		}
	}	
}
